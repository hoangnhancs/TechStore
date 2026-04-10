using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CommentService.Repositories.Interface;
using Shared.Core.EF.UnitOfWork;

namespace CommentService.Persistence
{
    public interface ICommentUnitOfWork : IUnitOfWork
    {
        public ICommentRepository CommentRepository { get; }
    }
}