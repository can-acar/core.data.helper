using System;
using System.Data;
using System.Threading.Tasks;
using Autofac;
using core.data.helper.extensions;
using Microsoft.EntityFrameworkCore.Storage;

namespace core.data.helper.infrastructures
{
#pragma warning disable CS8603
    public class UnitOfWork<TContext>: BaseUnitOfWork<TContext> where TContext:class, IDbContext, IDisposable
    {
        private readonly IComponentContext Scope;
        private IDbContextTransaction ContextTransaction;

        public UnitOfWork(IContextAdaptor<TContext> contextAdaptor, IComponentContext scope): base(contextAdaptor) { Scope = scope; }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <returns></returns>
        public override IDbContextTransaction BeginTransaction()
        {
            ContextTransaction = DataContext.Database.BeginTransaction();
            return ContextTransaction;
        }

        public override IDbContextTransaction BeginTransaction(IsolationLevel isolationLevel)
        {
            ContextTransaction = DataContext.Database.BeginTransaction();
            return ContextTransaction;
        }


        /// <summary>
        /// </summary>
        public override void Commit() { ContextTransaction?.Commit(); }

        /// <summary>
        /// </summary>
        public override void Rollback()
        {
            if(DataContext != null) ContextTransaction.Rollback();
        }

        /// <summary>
        /// </summary>
        /// <returns></returns>
        public override int SaveChanges()
        {
            if(DataContext != null) return DataContext.GetObjectContext().SaveChanges();

            return-1;
        }

        /// <summary>
        /// </summary>
        /// <returns></returns>
        public override Task<int> SaveChangesAsync()
        {
            return DataContext != null
                ? DataContext.GetObjectContext().SaveChangesAsync()
                : Task.FromResult(-1);
        }


        public override IRepository<TEntity> Repository<TEntity>()
        {
            var Container = Scope.Resolve<IRepository<TEntity>>();

            return Container;
        }
    }
}