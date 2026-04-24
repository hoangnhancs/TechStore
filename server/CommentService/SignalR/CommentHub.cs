using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using CommentService.Services.Comment;
using MediatR;
using Microsoft.AspNetCore.SignalR;

namespace CommentService.SignalR
{
    public class CommentHub : Hub
    {
        private readonly IMediator _mediator;
        public CommentHub(IMediator mediator)
        {
            _mediator = mediator;
        }

        public async Task<string?> SendComment(string productId, string content, string? parrentCommentId = null)
        {
            var userId = Context.User?.FindFirstValue(ClaimTypes.NameIdentifier);
            var userName = Context.User?.FindFirstValue(ClaimTypes.Name);
            var userImageUrl = Context.User?.FindFirstValue("image_url");
            if (string.IsNullOrEmpty(userId))
            {
                await Clients.Caller.SendAsync("CommentError", "User not authenticated");
                return null;
            }
            var command = new CreateCommentCommand
            {
                ProductId = productId,
                UserId = userId,
                UserName = userName,
                UserImageUrl = userImageUrl,
                ParentCommentId = parrentCommentId,
                Content = content
            };

            var result = await _mediator.Send(command);
            if (result.IsSuccess && result.Value != null)
            {
                await Clients.Group(productId).SendAsync("ReceiveComment", result.Value);
                return result.Value.Id;
            }
            else
            {
                await Clients.Caller.SendAsync("CommentError", result.Error);
                return null;
            }
            
        }

        public async Task LoadAllComments(string productId)
        {
            var query = new GetListCommentsByProductIdQuery { ProductId = productId };
            var result = await _mediator.Send(query);

            if (result.IsSuccess && result.Value != null)
            {
                await Clients.Caller.SendAsync("ReceiveAllComments", result.Value);
            }
            else
            {
                await Clients.Caller.SendAsync("CommentError", result.Error);
            }
        }
        public async Task JoinProductGroup(string productId)
        {
            if (string.IsNullOrEmpty(productId))
            {
                return;
            }
            await Groups.AddToGroupAsync(Context.ConnectionId, productId);
            Console.WriteLine($"Client {Context.ConnectionId} joined group {productId}"); // Log ở server
        }

        public async Task LeaveProductGroup(string productId) 
        {
            if (string.IsNullOrEmpty(productId))
            {
                return;
            }
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, productId);
            Console.WriteLine($"Client {Context.ConnectionId} left group {productId}"); 
        }
    }
}