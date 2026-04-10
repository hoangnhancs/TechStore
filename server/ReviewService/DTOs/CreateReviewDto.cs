using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ReviewService.DTOs
{
    public class CreateReviewDto
    {
        public required string ProductId { get; set; }
        public required string Content { get; set; }
        public required int Rating { get; set; }
    }
}