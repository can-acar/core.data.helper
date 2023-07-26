using System.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage;

namespace CoreEntityHelper.Infrastructures;
#pragma warning disable CS8603
public sealed class UnitOfWork<TContext> : IUnitOfWork<TContext>, IUnitOfWork where TContext : DbContext, IDisposable
{
    public TContext DbContext { get; }

    private IDbContextTransaction _contextTransaction;
    private bool _disposed;

    public UnitOfWork(TContext context)
    {
        DbContext = context ?? throw new ArgumentNullException(nameof(context));
        _contextTransaction = DbContext.Database.CurrentTransaction;
    }

    public async Task ExecuteAsync(Func<Task> action)
    {
        var strategy = DbContext.Database.CreateExecutionStrategy();
        await strategy.ExecuteAsync(async () => { await action(); });
    }

    public IDbContextTransaction BeginTransaction()
    {
        _contextTransaction = DbContext.Database.BeginTransaction();
        return _contextTransaction;
    }

    public IDbContextTransaction BeginTransaction(IsolationLevel isolationLevel)
    {
        _contextTransaction = DbContext.Database.BeginTransaction();
        return _contextTransaction;
    }

    public Task<IDbContextTransaction> BeginTransactionAsync() => DbContext.Database.BeginTransactionAsync();


    public Task<IDbContextTransaction> BeginTransactionAsync(IsolationLevel isolationLevel) =>
        DbContext.Database.BeginTransactionAsync(isolationLevel);

    public IRepository<TEntity> Repository<TEntity>() where TEntity : class
    {
        var customRepo = DbContext.GetService<IRepository<TEntity>>(); //.Resolve<IRepository<TEntity>>();

        return customRepo;
    }

    public void Rollback()
    {
        _contextTransaction.Rollback();
    }

    public int SaveChanges()
    {
        return DbContext.SaveChanges();
    }

    public Task<int> SaveChangesAsync()
    {
        return DbContext.SaveChangesAsync();
    }

    public void Commit()
    {
        _contextTransaction?.Commit();
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    private void Dispose(bool disposing)
    {
        if (!_disposed)
            if (disposing)
                DbContext.Dispose();

        _disposed = true;
    }
}