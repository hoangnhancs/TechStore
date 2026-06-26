using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Shared.Core.EF.Domain.Entities;
using Shared.Core.EF.Domain.Repositories;

namespace Infrastructure.EF.Repositories
{
    public class BaseEFRepository<T, TId, TContext> : IBaseEFRepository<T, TId>
         where T : class, IBaseEntity<TId>
         where TContext : DbContext
    {
        protected readonly TContext _dbContext;
        protected readonly DbSet<T> DbSet;

        public BaseEFRepository(TContext dbContext)
        {
            _dbContext = dbContext;
            DbSet = _dbContext.Set<T>();
        }

        public virtual async Task<T?> GetByIdAsync(TId id, CancellationToken cancellationToken = default)
        {
            return await DbSet.FindAsync(new object?[] { id }, cancellationToken);
        }

        public virtual async Task<IReadOnlyList<T>> GetListAsync(CancellationToken cancellationToken = default)
        {
            return await DbSet.Where(x => x.IsDeleted == false).ToListAsync(cancellationToken);
        }

        public virtual async Task<bool> AnyAsync(CancellationToken cancellationToken = default)
        {
            return await DbSet.Where(x => x.IsDeleted == false).AnyAsync(cancellationToken);
        }

        public virtual async Task<int> CountAsync(CancellationToken cancellationToken = default)
        {
            return await DbSet.Where(x => x.IsDeleted == false).CountAsync(cancellationToken);
        }

        public virtual async Task AddAsync(T entity, CancellationToken cancellationToken = default)
        {
            await DbSet.AddAsync(entity, cancellationToken);
        }

        public virtual void Update(T entity)
        {
            DbSet.Update(entity);
        }

        public virtual void Delete(T entity)
        {
            entity.MarkAsDeleted();
            DbSet.Update(entity);
        }

        public virtual async Task AddRangeAsync(IEnumerable<T> entities, CancellationToken cancellationToken = default)
        {
            await DbSet.AddRangeAsync(entities, cancellationToken);
        }
    }
}
