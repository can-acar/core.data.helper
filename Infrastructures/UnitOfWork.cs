using System;
using System.Data;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage;

namespace Core.Data.Helper.Infrastructures
{
#pragma warning disable CS8603
    public sealed class UnitOfWork<TContext> : IUnitOfWork<TContext>, IUnitOfWork where TContext : DbContext, IDisposable
    {
        private readonly TContext Context;
        public TContext DbContext => Context;
        private IDbContextTransaction ContextTransaction;
        private bool Disposed;

        public UnitOfWork(TContext context)
        {
            Context = context ?? throw new ArgumentNullException(nameof(context));
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
            if (Context != null) ContextTransaction.Rollback();
        }

        public int SaveChanges()
        {
            if (Context != null) return Context.SaveChanges();

            return -1;
        }

        public Task<int> SaveChangesAsync()
        {
            return Context != null ? Context.SaveChangesAsync() : Task.FromResult(-1);
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
                    Context.Dispose();

            Disposed = true;
        }
    }
}