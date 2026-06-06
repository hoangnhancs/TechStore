using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CommentService.DTOs;
using MediatR;
using Shared.Core.EF.Application;

namespace CommentService.Services.Comment
{
    public class CreateCommentCommand : IRequest<AppResult<CommentDto>>
    {
        public required string ReferenceId { get; set; } = string.Empty;
        public required string ReferenceType { get; set; } = string.Empty;
        public required string UserId { get; set; } = string.Empty;
        public string? UserName { get; set; }
        public string? UserImageUrl { get; set; }
        public required string Content { get; set; } = string.Empty;
        public string? ParentCommentId { get; set; } = null;
    }
}