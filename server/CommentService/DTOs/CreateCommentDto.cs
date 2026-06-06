using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CommentService.DTOs
{
    public class CreateCommentDto
    {
        public required string Content { get; set; }
        public string? ParentCommentId { get; set; } = null;
        public required string ReferenceId { get; set; } = string.Empty;
        public required string ReferenceType { get; set; } = string.Empty;
    }
}