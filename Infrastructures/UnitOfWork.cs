using System.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage;

namespace CoreEntityHelper.Infrastructures;
#pragma warning disable CS8603
public sealed class UnitOfWork<TContext> : IUnitOfWork<TContext>, IUnitOfWork where TContext : DbContext, IDisposable
{
    public TContext DbContext { get; }

    private IDbContextTransaction ContextTransaction;
    private bool Disposed;

    public UnitOfWork(TContext context)
    {
        DbContext = context ?? throw new ArgumentNullException(nameof(context));
        ContextTransaction = DbContext.Database.BeginTransaction();
    }

    public async Task ExecuteAsync(Func<Task> action)
    {
        var strategy = DbContext.Database.CreateExecutionStrategy();
        await strategy.ExecuteAsync(async () => { await action(); });
    }

    public IDbContextTransaction BeginTransaction()
    {
        ContextTransaction = DbContext.Database.BeginTransaction();
        return ContextTransaction;
    }

    public IDbContextTransaction BeginTransaction(IsolationLevel isolationLevel)
    {
        ContextTransaction = DbContext.Database.BeginTransaction();
        return ContextTransaction;
    }

    public IRepository<TEntity> Repository<TEntity>() where TEntity : class
    {
        var customRepo = DbContext.GetService<IRepository<TEntity>>(); //.Resolve<IRepository<TEntity>>();

        return customRepo;
    }

    public void Rollback()
    {
        ContextTransaction.Rollback();
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
        ContextTransaction?.Commit();
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    private void Dispose(bool disposing)
    {
        if (!Disposed)
            if (disposing)
                DbContext.Dispose();

        Disposed = true;
    }
}