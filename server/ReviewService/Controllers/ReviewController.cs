using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using ReviewService.DTOs;
using Microsoft.AspNetCore.Mvc;
using Shared.Web.Controller;
using ReviewService.Services.Review;

namespace ReviewService.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ReviewController : BaseApiController
    {
        [HttpPost("create")]
        public async Task<IActionResult> Create([FromBody] CreateReviewDto reviewDto)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized("User is not authenticated.");
            }
            return HandleAppResult(await Mediator.Send(new CreateReviewCommand
            {
                ProductId = reviewDto.ProductId,
                UserId = userId,
                Rating = reviewDto.Rating,
                Content = reviewDto.Content
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