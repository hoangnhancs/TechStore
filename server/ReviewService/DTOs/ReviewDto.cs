using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ReviewService.DTOs
{
    public class ReviewDto
    {
        public string? Id { get; set; }
        public required string ProductId { get; set; } = string.Empty;
        public required string UserId { get; set; } = string.Empty;
        public string? UserName { get; set; }
        public string? UserDisplayName { get; set; }
        public string? UserImageUrl { get; set; }
        public required int Rating { get; set; }
        public string? Content { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public bool HasAdminReply { get; set; } = false;
    }
}