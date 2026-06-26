using CommentService.Data;
using CommentService.Entities;
using CommentService.Repositories.Interface;
using Infrastructure.EF.Repositories;
using Microsoft.EntityFrameworkCore;

namespace CommentService.Repositories
{
    public class CommentRepository : BaseEFRepository<Comment, string, CommentSvcDbContext>, ICommentRepository
    {
        public CommentRepository(CommentSvcDbContext dbContext) : base(dbContext)
        {
        }

        public async Task<string?> GetUserIdByCommentIdAsync(string commentId, CancellationToken cancellationToken = default)
        {
            return await DbSet
                .Where(x => x.Id == commentId)
                .Select(x => x.UserId)
                .FirstOrDefaultAsync(cancellationToken);
        }

        public async Task<List<Comment>> GetByProductIdAsync(string productId, CancellationToken cancellationToken = default)
        {
            return await DbSet
                .Where(c => c.ReferenceId == productId && c.ReferenceType == Comment.ReferenceTypes.Product.ToString())
                .ToListAsync(cancellationToken);
        }
    }
}