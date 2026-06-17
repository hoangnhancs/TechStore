using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Contract.Comment
{
    public class CommentCreated
    {
        public required string CommentId { get; set; }
        public required string Content { get; set; }
        //public string? Title { get; set; }
        //public required string ReferenceType { get; set; }
        //public required string ReferenceId { get; set; }
        public string? Link { get; set; }
        public string? ParentCommentId { get; set; } = null;
        public string? ParantCommentUserId { get; set; } = null;
        public DateTime CreatedAt { get; set; }
        public required string UserId { get; set; }
    }
}
