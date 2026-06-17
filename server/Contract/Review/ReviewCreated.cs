using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Contract.Review
{
    public class ReviewCreated
    {
        public required string ReviewId { get; set; }
        public required string ProductId { get; set; }
        public required string UserId { get; set; }
        public required string Content { get; set; }
        public required int Rating { get; set; }
    }
}