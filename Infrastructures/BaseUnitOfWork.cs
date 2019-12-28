using System;
using System.Data;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

namespace core.data.helper.infrastructures
{
    /// <summary>
    /// </summary>
    /// <typeparam name="TContext"></typeparam>
    public abstract class BaseUnitOfWork<TContext> : IUnitOfWork where TContext : class, IDisposable
    {
        private readonly IContextAdaptor<TContext> contextAdaptor;
        private TContext context;
        private bool disposed;

        /// <summary>
        /// </summary>
        /// <param name="contextAdaptor"></param>
        protected BaseUnitOfWork(IContextAdaptor<TContext> contextAdaptor)
        {
            this.contextAdaptor = contextAdaptor;
        }

        protected bool LazyLoadingEnabled
        {
            set => (context as DbContext).ChangeTracker.LazyLoadingEnabled = value; //DbContext().ChangeTracker.LazyLoadingEnabled = value;
            get => (context as DbContext).ChangeTracker.LazyLoadingEnabled;
        }

        protected TContext DataContext
        {
            get
            {
                if (context != null)
                    return context;

                context = contextAdaptor.GetContext();

                return context;
            }
        }
#pragma warning disable CS8603
        public DbContext DbContext()
        {
            return context as DbContext;
        }

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
            if (!disposed)
                if (disposing)
                    DataContext.Dispose();

            disposed = true;
        }
    }
}