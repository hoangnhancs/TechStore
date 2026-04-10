using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CommentService.Data;
using CommentService.Repositories.Interface;
using Infrastructure.EF.UnitOfWork;

namespace CommentService.Persistence
{
    public class CommentUnitOfWork : UnitOfWork<CommentSvcDbContext>, ICommentUnitOfWork
    {
        public ICommentRepository CommentRepository { get; }
        public CommentUnitOfWork(CommentSvcDbContext context,
            ICommentRepository commentRepository) : base(context)
        {
            CommentRepository = commentRepository;
        }
    }
}