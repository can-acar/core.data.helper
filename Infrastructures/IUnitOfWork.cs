using System.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

namespace CoreEntityHelper.Infrastructures;

public interface IUnitOfWork<out TContext> : IUnitOfWork where TContext : DbContext
{
    TContext DbContext { get; }
}

public interface IUnitOfWork : IDisposable
{
    IDbContextTransaction BeginTransaction();

    IDbContextTransaction BeginTransaction(IsolationLevel isolationLevel);

    Task<IDbContextTransaction> BeginTransactionAsync();

    Task<IDbContextTransaction> BeginTransactionAsync(IsolationLevel isolationLevel);

    Task<IQueryable<TEntity>> SqlQueryAsync<TEntity>(string query, params object[] parameters);


    IRepository<TEntity> Repository<TEntity>() where TEntity : class;

    void Rollback();

    int SaveChanges();

    Task<int> SaveChangesAsync();

    void Commit();
}