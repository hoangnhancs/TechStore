using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using MongoDB.Driver;
using MongoDB.Entities;
using Shared.Core.Mongo.Domain.Entities;
using Shared.Core.Mongo.Domain.Repositories;


namespace Infrastructure.Mongo.Repositories
{
    /// <summary>
    /// Base repository implementation for MongoDB using MongoDB.Entities library
    /// </summary>
    public class BaseMongoRepository<T> : IBaseMongoRepository<T, string>
        where T : MongoEntity
    {
        public BaseMongoRepository()
        {
            // MongoDB.Entities manages connections globally via DB.InitAsync
        }

        public virtual async Task<T?> GetByIdAsync(string id, CancellationToken cancellationToken = default)
        {
            return await DB.Find<T>().OneAsync(id, cancellationToken );
        }

        public virtual async Task<T?> FindOneAsync(
            Expression<Func<T, bool>> filter,
            CancellationToken cancellationToken = default)
        {
            return await DB.Find<T>()
                .Match(filter)
                .ExecuteSingleAsync(cancellationToken);
        }

        public virtual async Task<IEnumerable<T>> GetListAsync(
            Expression<Func<T, bool>>? filter = null,
            int? skip = null,
            int? limit = null,
            CancellationToken cancellationToken = default)
        {
            var query = DB.Find<T>();

            if (filter != null)
                query.Match(filter);

            if (skip.HasValue)
                query.Skip(skip.Value);

            if (limit.HasValue)
                query.Limit(limit.Value);

            return await query.ExecuteAsync(cancellationToken);
        }

        public virtual async Task<IEnumerable<T>> GetListAsync(
            FilterDefinition<T> filter,
            int? skip = null,
            int? limit = null,
            CancellationToken cancellationToken = default)
        {
            var query = DB.Find<T>()
                .Match(filter);

            if (skip.HasValue)
                query.Skip(skip.Value);

            if (limit.HasValue)
                query.Limit(limit.Value);

            return await query.ExecuteAsync(cancellationToken);
        }

        public virtual async Task<bool> AnyAsync(
            Expression<Func<T, bool>> filter,
            CancellationToken cancellationToken = default)
        {
            return await DB.Find<T>()
                .Match(filter)
                .ExecuteAnyAsync(cancellationToken);
        }

        public virtual async Task<long> CountAsync(
            Expression<Func<T, bool>>? filter = null,
            CancellationToken cancellationToken = default)
        {
            if (filter == null)
                return await DB.CountAsync<T>(cancellation: cancellationToken);

            return await DB.CountAsync(filter, cancellation: cancellationToken);
        }

        public virtual async Task<long> CountAsync(
            FilterDefinition<T> filter,
            CancellationToken cancellationToken = default)
        {
            return await DB.CountAsync(filter, cancellation: cancellationToken);
        }

        public virtual async Task<string> AddAsync(T entity, CancellationToken cancellationToken = default)
        {
            await entity.SaveAsync(cancellation: cancellationToken);
            return entity.ID;
        }

        public virtual async Task AddManyAsync(IEnumerable<T> entities, CancellationToken cancellationToken = default)
        {
            await entities.SaveAsync(cancellation: cancellationToken);
        }

        public virtual async Task UpdateAsync(T entity, CancellationToken cancellationToken = default)
        {
            await entity.SaveAsync(cancellation: cancellationToken);
        }

        public virtual async Task<bool> UpdateAsync(
            Expression<Func<T, bool>> filter,
            UpdateDefinition<T> update,
            CancellationToken cancellationToken = default)
        {
            var result = await DB.Update<T>()
                .Match(filter)
                .Modify(b => update)
                .ExecuteAsync(cancellation: cancellationToken);

            return result.IsAcknowledged && result.ModifiedCount > 0;
        }

        public virtual async Task DeleteAsync(string id, CancellationToken cancellationToken = default)
        {
            await DB.DeleteAsync<T>(id, cancellation: cancellationToken);
        }

        public virtual async Task DeleteAsync(
            Expression<Func<T, bool>> filter,
            CancellationToken cancellationToken = default)
        {
            await DB.DeleteAsync(filter, cancellation: cancellationToken);
        }

        public virtual async Task<long> DeleteManyAsync(
            Expression<Func<T, bool>> filter,
            CancellationToken cancellationToken = default)
        {
            var result = await DB.DeleteAsync(filter, cancellation: cancellationToken);
            return result.DeletedCount;
        }
    }
}
