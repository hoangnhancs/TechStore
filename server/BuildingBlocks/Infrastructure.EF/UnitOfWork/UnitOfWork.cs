using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Shared.Core.EF.UnitOfWork;

namespace Infrastructure.EF.UnitOfWork
{
    public class UnitOfWork<TContext> : IUnitOfWork where TContext : DbContext
    {
        protected readonly TContext Context;
        private IDbContextTransaction? _currentTransaction;
        public UnitOfWork(TContext context)
        {
            Context = context ?? throw new ArgumentNullException(nameof(context));
        }
        public virtual async Task BeginTransactionAsync(CancellationToken cancellationToken = default)
        {
            if (_currentTransaction != null)
            {
                throw new InvalidOperationException("A transaction is already in progress");
            }

            _currentTransaction = await Context.Database.BeginTransactionAsync(cancellationToken);
        }

        public virtual async Task<bool> CommitAsync(CancellationToken cancellationToken = default)
        {
            try
            {
                var result = await Context.SaveChangesAsync(cancellationToken);
                return result > 0;
            }
            catch (DbUpdateConcurrencyException ex)
            {
                // Handle concurrency conflicts
                throw new InvalidOperationException("Concurrency conflict occurred", ex);
            }
            catch (DbUpdateException ex)
            {
                // Handle database update errors
                throw new InvalidOperationException("Database update failed", ex);
            }
        }

        public virtual async Task CommitTransactionAsync(CancellationToken cancellationToken = default)
        {
            if (_currentTransaction == null)
            {
                throw new InvalidOperationException("No transaction in progress");
            }

            try
            {
                await Context.SaveChangesAsync(cancellationToken);
                await _currentTransaction.CommitAsync(cancellationToken);
            }
            catch
            {
                await RollbackAsync(cancellationToken);
                throw;
            }
            finally
            {
                if (_currentTransaction != null)
                {
                    await _currentTransaction.DisposeAsync();
                    _currentTransaction = null;
                }
            }
        }

        public virtual void Dispose()
        {
            _currentTransaction?.Dispose();
            Context.Dispose();
        }

        public virtual async Task RollbackAsync(CancellationToken cancellationToken = default)
        {
            if (_currentTransaction != null)
            {
                await _currentTransaction.RollbackAsync(cancellationToken);
                await _currentTransaction.DisposeAsync();
                _currentTransaction = null;
            }

            // Discard changes in tracked entities
            foreach (var entry in Context.ChangeTracker.Entries())
            {
                switch (entry.State)
                {
                    case EntityState.Added:
                        entry.State = EntityState.Detached;
                        break;
                    case EntityState.Modified:
                    case EntityState.Deleted:
                        await entry.ReloadAsync(cancellationToken);
                        break;
                }
            }
        }
    }
}