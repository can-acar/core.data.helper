using System;
using System.Data;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

namespace Core.Data.Helper.Infrastructures
{
    public interface IUnitOfWork<TContext> : IUnitOfWork where TContext : DbContext
    {
        TContext DbContext { get; }
    }

    public interface IUnitOfWork : IDisposable
    {
        IDbContextTransaction BeginTransaction();

        IDbContextTransaction BeginTransaction(IsolationLevel isolationLevel);

        IRepository<TEntity> Repository<TEntity>() where TEntity : class;

        void Rollback();

        int SaveChanges();

        Task<int> SaveChangesAsync();

        void Commit();
    }
}