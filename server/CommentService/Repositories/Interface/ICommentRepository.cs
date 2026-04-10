using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CommentService.Entities;
using Shared.Core.EF.Domain.Repositories;

namespace CommentService.Repositories.Interface
{
    public interface ICommentRepository : IBaseEFRepository<Comment, string>
    {
        
    }
}