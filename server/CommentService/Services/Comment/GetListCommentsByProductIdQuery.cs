using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CommentService.DTOs;
using MediatR;
using Shared.Core.EF.Application;

namespace CommentService.Services.Comment
{
    public class GetListCommentsByProductIdQuery : IRequest<AppResult<List<CommentDto>>>
    {
        public required string ProductId { get; set; }
    }
}