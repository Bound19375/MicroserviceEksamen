using System.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

namespace Crosscutting.TransactionHandling
{
    public class UnitOfWork<T> : IUnitOfWork<T> where T : DbContext
    {
        private readonly T _context;
        private IDbContextTransaction _transaction = null!;

        public UnitOfWork(T context)
        {
            _context = context;
        }
        async Task IUnitOfWork<T>.CreateTransaction(IsolationLevel isolationLevel)
        {
            try 
            {
                _transaction = _context.Database.CurrentTransaction ?? await _context.Database.BeginTransactionAsync(isolationLevel: isolationLevel);
            }
            catch (Exception exception) {
                throw new Exception(exception.ToString());
            }
        }

        async Task IUnitOfWork<T>.Commit()
        {
            try
            {
                await _transaction.CommitAsync();
                await _transaction.DisposeAsync();
            }
            catch (Exception exception)
            {
                throw new Exception(exception.ToString());
            }
        }

        async Task IUnitOfWork<T>.Rollback()
        {
            try
            {
                await _transaction.RollbackAsync();
                await _transaction.DisposeAsync();
            }
            catch (Exception exception)
            {
                throw new Exception(exception.ToString());
            }
        }
    }
}
