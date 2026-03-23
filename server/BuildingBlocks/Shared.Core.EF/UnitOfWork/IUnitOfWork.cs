using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Shared.Core.EF.UnitOfWork
{
    public interface IUnitOfWork : IDisposable
    {
        /// <summary>
        /// Saves all pending changes to the database
        /// </summary>
        Task<bool> CommitAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// Rollback changes (if transaction is used)
        /// </summary>
        Task RollbackAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// Begin a transaction
        /// </summary>
        Task BeginTransactionAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// Commit the current transaction
        /// </summary>
        Task CommitTransactionAsync(CancellationToken cancellationToken = default);
    }
}