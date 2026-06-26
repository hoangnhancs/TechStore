using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Shared.Core.EF.Domain.Entities;

namespace Shared.Core.EF.Domain.Repositories
{
    public interface IBaseEFRepository<T, TId> where T : class, IBaseEntity<TId>
    {
        Task<T?> GetByIdAsync(TId id, CancellationToken cancellationToken = default);
        Task<IReadOnlyList<T>> GetListAsync(CancellationToken cancellationToken = default);
        Task<bool> AnyAsync(CancellationToken cancellationToken = default);
        Task<int> CountAsync(CancellationToken cancellationToken = default);
        Task AddAsync(T entity, CancellationToken cancellationToken = default);
        Task AddRangeAsync(IEnumerable<T> entities, CancellationToken cancellationToken = default);
        void Update(T entity);
        void Delete(T entity);
    }
}
