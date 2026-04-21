using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CommentService.Data;
using CommentService.Repositories;
using CommentService.Repositories.Interface;
using Infrastructure.EF.UnitOfWork;

namespace CommentService.Persistence
{
    public class CommentUnitOfWork : UnitOfWork<CommentSvcDbContext>, ICommentUnitOfWork
    {
        private ICommentRepository? _commentRepository;
        public ICommentRepository CommentRepository => 
            _commentRepository ??= new CommentRepository(_dbContext);
        public CommentUnitOfWork(CommentSvcDbContext context) : base(context)
        {
        }
    }
}