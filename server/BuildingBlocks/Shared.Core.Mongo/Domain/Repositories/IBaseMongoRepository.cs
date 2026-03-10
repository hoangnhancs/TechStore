using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using MongoDB.Driver;
using Shared.Core.Mongo.Domain.Entities;

namespace Shared.Core.Mongo.Domain.Repositories
{
    /// <summary>
    /// Base repository interface for MongoDB entities using MongoDB.Entities library
    /// </summary>
    public interface IBaseMongoRepository<T, TId> where T : class, IEntity<TId>
    {
        // Query operations
        Task<T?> GetByIdAsync(TId id, CancellationToken cancellationToken = default);
        
        Task<T?> FindOneAsync(
            Expression<Func<T, bool>> filter,
            CancellationToken cancellationToken = default);

        Task<IEnumerable<T>> GetListAsync(
            Expression<Func<T, bool>>? filter = null,
            int? skip = null,
            int? limit = null,
            CancellationToken cancellationToken = default);

        Task<IEnumerable<T>> GetListAsync(
            FilterDefinition<T> filter,
            int? skip = null,
            int? limit = null,
            CancellationToken cancellationToken = default);

        Task<bool> AnyAsync(
            Expression<Func<T, bool>> filter,
            CancellationToken cancellationToken = default);

        Task<long> CountAsync(
            Expression<Func<T, bool>>? filter = null,
            CancellationToken cancellationToken = default);

        Task<long> CountAsync(
            FilterDefinition<T> filter,
            CancellationToken cancellationToken = default);

        // Command operations
        Task<string> AddAsync(T entity, CancellationToken cancellationToken = default);
        
        Task AddManyAsync(IEnumerable<T> entities, CancellationToken cancellationToken = default);

        Task UpdateAsync(T entity, CancellationToken cancellationToken = default);

        Task<bool> UpdateAsync(
            Expression<Func<T, bool>> filter,
            UpdateDefinition<T> update,
            CancellationToken cancellationToken = default);

        Task DeleteAsync(TId id, CancellationToken cancellationToken = default);

        Task DeleteAsync(Expression<Func<T, bool>> filter, CancellationToken cancellationToken = default);

        Task<long> DeleteManyAsync(
            Expression<Func<T, bool>> filter,
            CancellationToken cancellationToken = default);
    }
}
