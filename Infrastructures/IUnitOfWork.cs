using System;
using System.Data;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

namespace core.data.helper.infrastructures
{

    public interface IUnitOfWork : IDisposable
    {
        IDbContextTransaction BeginTransaction();

        IDbContextTransaction BeginTransaction(IsolationLevel isolationLevel);

        IRepository<TEntity> Repository<TEntity>() where TEntity:class;

        void Rollback();

        int SaveChanges();

        Task<int> SaveChangesAsync();

        void Commit();

        DbContext DbContext();
    }

}