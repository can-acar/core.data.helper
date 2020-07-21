using System.Data;
using System.Threading.Tasks;
using Autofac;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage;

namespace Core.Data.Helper.Infrastructures
{
#pragma warning disable CS8603
    public class UnitOfWork<TContext> : BaseUnitOfWork<TContext>, IUnitOfWork where TContext : DbContext
    {
        private readonly TContext Context;
        private readonly IComponentContext Scope;
        private IDbContextTransaction ContextTransaction;


        public UnitOfWork(TContext context, IComponentContext scope) : base(context)
        {
            Context = context;
            Scope   = scope;
        }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <returns></returns>
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

        /// <summary>
        /// </summary>
        public override void Commit()
        {
            ContextTransaction?.Commit();
        }

        /// <summary>
        /// </summary>
        public override void Rollback()
        {
            if (DbContext != null) ContextTransaction.Rollback();
        }

        /// <summary>
        /// </summary>
        /// <returns></returns>
        public override int SaveChanges()
        {
            if (DbContext != null) return DbContext.SaveChanges();

            return -1;
        }

        /// <summary>
        /// </summary>
        /// <returns></returns>
        public override Task<int> SaveChangesAsync()
        {
            return DbContext != null ? DbContext.SaveChangesAsync() : Task.FromResult(-1);
        }

        public override IRepository<TEntity> Repository<TEntity>()
        {
            var customRepo = Context.GetService<IRepository<TEntity>>(); //.Resolve<IRepository<TEntity>>();

            return customRepo;
        }
    }
}