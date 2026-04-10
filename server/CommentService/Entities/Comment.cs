using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Shared.Core.EF.Domain.Entities;

namespace CommentService.Entities
{
    public class Comment : BaseEntity<string>
    {
        public required string Content { get; set; }
        public bool IsVisible { get; set; } = true;
        public bool IsEdited { get; set; } = false;
        public required string UserId { get; set; }
        public required string ProductId { get; set; }
        public string? ParentCommentId { get; set; }
        public Comment? ParentComment { get; set; }
        public List<Comment> Replies { get; set; } = [];
        public bool IsAdminComment { get; set; } = false;
        public bool HasAdminReply { get; set; } = false;
        public bool CanReply(string userId, bool isAdmin)
        {
            return UserId == userId || isAdmin;
        }
        public Comment() : base(Guid.NewGuid().ToString())
        {
        }
    }   
}