using System;
using System.Data;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

namespace Core.Data.Helper.Infrastructures
{
    /// <summary>
    /// </summary>
    /// <typeparam name="TContext"></typeparam>
    public abstract class BaseUnitOfWork<TContext> : IUnitOfWork<TContext> where TContext : DbContext
    {
        //private readonly IContextAdaptor<TContext> ContextAdaptor;
        private readonly TContext Context;
        private bool Disposed;

        protected bool LazyLoadingEnabled
        {
            set => Context.ChangeTracker.LazyLoadingEnabled = value; //DbContext().ChangeTracker.LazyLoadingEnabled = value;
            get => Context.ChangeTracker.LazyLoadingEnabled;
        }

        public TContext DbContext => Context;

        /// <summary>
        /// </summary>
        /// <param name="contextAdaptor"></param>
        protected BaseUnitOfWork(TContext contextAdaptor)
        {
            Context = contextAdaptor;
        }


#pragma warning disable CS8603
        public DbContext CurrentContext()
        {
            return Context as DbContext;
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
            if (!Disposed)
                if (disposing)
                    DbContext.Dispose();

            Disposed = true;
        }
    }
}