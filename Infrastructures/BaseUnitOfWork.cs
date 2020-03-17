using System;
using System.Data;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

namespace core.data.helper.Infrastructures
{
    /// <summary>
    /// </summary>
    /// <typeparam name="TContext"></typeparam>
    public abstract class BaseUnitOfWork<TContext> : IUnitOfWork where TContext : class, IDisposable
    {
        private readonly IContextAdaptor<TContext> ContextAdaptor;
        private TContext Context;
        private bool Disposed;

        /// <summary>
        /// </summary>
        /// <param name="contextAdaptor"></param>
        protected BaseUnitOfWork(IContextAdaptor<TContext> contextAdaptor) { ContextAdaptor = contextAdaptor; }

        protected bool LazyLoadingEnabled
        {
            set => (Context as DbContext).ChangeTracker.LazyLoadingEnabled = value; //DbContext().ChangeTracker.LazyLoadingEnabled = value;
            get => (Context as DbContext).ChangeTracker.LazyLoadingEnabled;
        }

        protected TContext DataContext
        {
            get
            {
                if (Context != null)
                    return Context;

                Context = ContextAdaptor.GetContext();

                return Context;
            }
        }
#pragma warning disable CS8603
        public DbContext DbContext() { return Context as DbContext; }

        /// <summary>
        /// </summary>
        public abstract IDbContextTransaction BeginTransaction();

        public abstract IDbContextTransaction BeginTransaction(IsolationLevel isolationLevel);

        public abstract IRepository<TEntity> Repository<TEntity>() where TEntity : class;

        public abstract void Commit();

        public abstract void Rollback();

        public abstract int SaveChanges();

        public abstract Task<int> SaveChangesAsync();

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!Disposed)
                if (disposing)
                    DataContext.Dispose();

            Disposed = true;
        }
    }
}