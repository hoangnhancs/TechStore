using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Shared.Core.EF.Domain.Entities;

namespace Shared.Core.EF.Domain.Repositories
{
    public interface IBaseEFRepository<T, TId> where T : class, IBaseEntity<TId>
    {
        // Query operations
        Task<T?> GetByIdAsync(TId id, CancellationToken cancellationToken = default);
        Task<T?> GetByIdAsync(TId id, Func<IQueryable<T>, IQueryable<T>>? query, CancellationToken cancellationToken = default);
        IQueryable<T> GetAll();
        Task<IEnumerable<T>> GetListAsync(
            Expression<Func<T, bool>>? predicate = null,
            Func<IQueryable<T>, IQueryable<T>>? query = null,
            CancellationToken cancellationToken = default);

        Task<bool> AnyAsync(
            Expression<Func<T, bool>> predicate,
            CancellationToken cancellationToken = default);

        Task<int> CountAsync(
            Expression<Func<T, bool>>? predicate = null,
            CancellationToken cancellationToken = default);

        // Command operations
        Task AddAsync(T entity, CancellationToken cancellationToken = default);
        void Update(T entity);
        void Delete(T entity);
    }
}