using System;
using System.Data;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

namespace Core.Data.Helper.Infrastructures
{
    public abstract class BaseUnitOfWork<TContext> : IUnitOfWork<TContext>, IUnitOfWork where TContext : DbContext
    {
        private readonly TContext Context;

        TContext IUnitOfWork<TContext>.DbContext => Context;

        private bool Disposed;

        protected bool LazyLoadingEnabled
        {
            set => Context.ChangeTracker.LazyLoadingEnabled = value;
            get => Context.ChangeTracker.LazyLoadingEnabled;
        }

        protected BaseUnitOfWork(TContext context)
        {
            Context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public DbContext CurrentContext()
        {
            return Context;
        }


        public abstract IDbContextTransaction BeginTransaction();


        public abstract IDbContextTransaction BeginTransaction(IsolationLevel isolationLevel);


        public abstract IRepository<TEntity> Repository<TEntity>() where TEntity : class;


        public abstract void Rollback();

        public abstract int SaveChanges();


        public abstract Task<int> SaveChangesAsync();


        public abstract void Commit();


        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!Disposed)
                if (disposing)
                    Context.Dispose();

            Disposed = true;
        }
    }
}