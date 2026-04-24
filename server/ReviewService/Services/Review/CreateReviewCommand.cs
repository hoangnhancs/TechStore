using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MediatR;
using ReviewService.DTOs;
using Shared.Core.EF.Application;

namespace ReviewService.Services.Review
{
    public class CreateReviewCommand : IRequest<AppResult<ReviewDto>>
    {
        public required string UserId { get; set; }
        public string? UserName { get; set; }
        public string? UserImageUrl { get; set; }
        public required string ProductId { get; set; }
        public required string Content { get; set; }
        public required int Rating { get; set; }
    }
}