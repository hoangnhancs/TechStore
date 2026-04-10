using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using CommentService.DTOs;
using CommentService.Services.Comment;
using Microsoft.AspNetCore.Mvc;
using Shared.Web.Controller;

namespace CommentService.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CommentController : BaseApiController
    {
        [HttpPost("create")]
        public async Task<IActionResult> Create([FromBody] CreateCommentDto commentDto)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized("User is not authenticated.");
            }
            return HandleAppResult(await Mediator.Send(new CreateCommentCommand
            {
                ProductId = commentDto.ProductId,
                UserId = userId,
                ParentCommentId = commentDto.ParentCommentId,
                Content = commentDto.Content
            }));
        }
        // [HttpGet("comments/{commentId}")]
        // public async Task<IActionResult> GetCommentById(string commentId)
        // {
        //     return HandleAppResult(await Mediator.Send(new GetCommentByIdQuery { CommentId = commentId }));
        // }
        // [HttpGet("comments")]
        // public async Task<IActionResult> GetCommentsByProductId([FromQuery]string productId)
        // {
        //     return HandleAppResult(await Mediator.Send(new GetListCommentsByProductIdQuery { ProductId = productId }));

        // }
    }
}