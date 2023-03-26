using Microsoft.EntityFrameworkCore;
using System.Data;

namespace Crosscutting.TransactionHandling;

public interface IUnitOfWork<T> where T : DbContext
{
    Task CreateTransaction(IsolationLevel level);
    Task Commit();
    Task Rollback();
}