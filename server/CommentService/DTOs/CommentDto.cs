using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CommentService.DTOs
{
    public class CommentDto
    {
        public required string Id { get; set; }
        public required string Content { get; set; }
        public required DateTime CreatedAt { get; set; }
        public required DateTime UpdatedAt { get; set; }
        public bool IsVisible { get; set; } = true;
        public bool IsEdited { get; set; } = false;
        public required string UserId { get; set; }
        public string? UserDisplayName { get; set; }
        public string? UserImageUrl { get; set; }
        //public required string ProductId { get; set; }
        public required string ReferenceId { get; set; }
        public required string ReferenceType { get; set; }
        public string? ParentCommentId { get; set; }
        public List<CommentDto> Replies { get; set; } = [];
        public bool IsAdminComment { get; set; } = false;
        public bool HasAdminReply { get; set; } = false;
    }
}