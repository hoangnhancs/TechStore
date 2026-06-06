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
        public string? ParentCommentId { get; set; }
        public Comment? ParentComment { get; set; }
        public List<Comment> Replies { get; set; } = [];
        public bool IsAdminComment { get; set; } = false;
        public bool HasAdminReply { get; set; } = false;
        public string ReferenceType { get; set; } = ReferenceTypes.Product.ToString();
        public string ReferenceId { get; set; } = string.Empty;
        public bool CanReply(string userId, bool isAdmin)
        {
            return UserId == userId || isAdmin;
        }
        public enum ReferenceTypes
        {
            Product,
        }
        public Comment() : base(Guid.NewGuid().ToString())
        {
        }
    }   
}