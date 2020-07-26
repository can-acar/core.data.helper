using System;
using System.Data;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage;

namespace Core.Data.Helper.Infrastructures
{
#pragma warning disable CS8603
    public class UnitOfWork<TContext> : BaseUnitOfWork<TContext>, IUnitOfWork<TContext>, IUnitOfWork where TContext : DbContext
    {
        private readonly TContext DbContext;

        private IDbContextTransaction ContextTransaction;


        public UnitOfWork(TContext context) : base(context)
        {
            DbContext = context ?? throw new ArgumentNullException(nameof(context));
        }


        public async Task ExecuteAsync(Func<Task> action)
        {
            var Strategy = DbContext.Database.CreateExecutionStrategy();
            await Strategy.ExecuteAsync(async () =>
            {
                action();

            });
        }

        public override IDbContextTransaction BeginTransaction()
        {
            ContextTransaction = DbContext.Database.BeginTransaction();
            return ContextTransaction;
        }

        public override IDbContextTransaction BeginTransaction(IsolationLevel isolationLevel)
        {
            ContextTransaction = DbContext.Database.BeginTransaction();
            return ContextTransaction;
        }


        public override void Commit()
        {
            ContextTransaction?.Commit();
        }


        public override void Rollback()
        {
            if (DbContext != null) ContextTransaction.Rollback();
        }


        public override int SaveChanges()
        {
            if (DbContext != null) return DbContext.SaveChanges();

            return -1;
        }

        public override Task<int> SaveChangesAsync()
        {
            return DbContext != null ? DbContext.SaveChangesAsync() : Task.FromResult(-1);
        }

        public override IRepository<TEntity> Repository<TEntity>()
        {
            var customRepo = DbContext.GetService<IRepository<TEntity>>(); //.Resolve<IRepository<TEntity>>();

            return customRepo;
        }
    }
}