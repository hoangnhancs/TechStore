using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CommentService.Data;
using CommentService.Entities;
using CommentService.Repositories.Interface;
using Infrastructure.EF.Repositories;

namespace CommentService.Repositories
{
    public class CommentRepository : BaseEFRepository<Comment, string, CommentSvcDbContext>, ICommentRepository
    {
        public CommentRepository(CommentSvcDbContext dbContext) : base(dbContext)
        {
        }
    }
}