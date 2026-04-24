using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using MediatR;
using Microsoft.AspNetCore.SignalR;
using ReviewService.Services.Review;

namespace ReviewService.SignalR
{
    public class ReviewHub : Hub
    {
        private readonly IMediator _mediator;
        public ReviewHub(IMediator mediator)
        {
            _mediator = mediator;
        }
        public async Task<string?> SendReview(string productId, string content, int rating)
        {
            var userId = Context.User?.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                await Clients.Caller.SendAsync("ReviewError", "User not authenticated");
                return null;
            }
            var userName = Context.User?.FindFirstValue(ClaimTypes.Name);
            var userImageUrl = Context.User?.FindFirstValue("image_url");
            var command = new CreateReviewCommand
            {
                ProductId = productId,
                UserId = userId,
                UserName = userName,
                UserImageUrl = userImageUrl,
                Content = content,
                Rating = rating,
            };

            var result = await _mediator.Send(command);
            if (result.IsSuccess && result.Value != null)
            {
                await Clients.Group(productId).SendAsync("ReceiveReview", result.Value);
                return result.Value.Id;
            }
            else
            {
                await Clients.Caller.SendAsync("ReviewError", string.IsNullOrEmpty(result.Error) ? "Failed to send review" : result.Error);
                return null;
            }
        }

        public async Task LoadAllReviews(string productId)
        {            
            if (string.IsNullOrEmpty(productId))
            {
                await Clients.Caller.SendAsync("ReviewError", "Invalid product ID");
                return;
            }
            var query = new GetListReviewsByProductIdQuery { ProductId = productId };
            var result = await _mediator.Send(query);

            if (result.IsSuccess && result.Value != null)
            {
                await Clients.Caller.SendAsync("ReceiveAllReviews", result.Value);
            }
            else
            {
                await Clients.Caller.SendAsync("ReviewError", string.IsNullOrEmpty(result.Error) ? "Failed to load reviews" : result.Error);
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