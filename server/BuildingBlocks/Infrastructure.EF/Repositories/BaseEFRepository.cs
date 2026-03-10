using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
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
        protected readonly TContext Context;
        protected readonly DbSet<T> DbSet;

        public BaseEFRepository(TContext context)
        {
            Context = context;
            DbSet = Context.Set<T>();
        }

        public virtual async Task<T?> GetByIdAsync(TId id, CancellationToken cancellationToken = default)
        {
            return await DbSet.FindAsync(id, cancellationToken);
        }

        public virtual async Task<T?> GetByIdAsync(
            TId id,
            Func<IQueryable<T>, IQueryable<T>>? include,
            CancellationToken cancellationToken = default)
        {
            IQueryable<T> query = DbSet;

            if (include != null)
                query = include(query);

            return await query.FirstOrDefaultAsync(e => e.Id!.Equals(id), cancellationToken);
        }

        public virtual async Task<IEnumerable<T>> GetListAsync(
            Expression<Func<T, bool>>? predicate = null,
            Func<IQueryable<T>, IQueryable<T>>? query = null,
            CancellationToken cancellationToken = default)
        {
            IQueryable<T> dbQuery = DbSet.AsQueryable();

            if (predicate != null)
                dbQuery = dbQuery.Where(predicate);

            if (query != null)
                dbQuery = query(dbQuery);

            return await dbQuery.ToListAsync(cancellationToken);
        }

        public virtual async Task<bool> AnyAsync(
            Expression<Func<T, bool>> predicate,
            CancellationToken cancellationToken = default)
        {
            return await DbSet.AnyAsync(predicate, cancellationToken);
        }

        public virtual async Task<int> CountAsync(
            Expression<Func<T, bool>>? predicate = null,
            CancellationToken cancellationToken = default)
        {
            if (predicate == null)
                return await DbSet.CountAsync(cancellationToken);

            return await DbSet.CountAsync(predicate, cancellationToken);
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
    }
}