using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MediatR;
using ReviewService.DTOs;
using Shared.Core.EF.Application;

namespace ReviewService.Services.Review
{
    public class GetListReviewsByProductIdQuery : IRequest<AppResult<List<ReviewDto>>>
    {
        public required string ProductId { get; set; }
    }
}