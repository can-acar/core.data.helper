using System.Data;
using core.data.helper.infrastructures;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

namespace core.data.helper.extensions
{
#pragma warning disable CS8603
    public static class DbContextExtensions
    {
        public static DbContext GetObjectContext(this IDbContext dbContext)
        {
            return dbContext as DbContext;
        }

        public static IDbContextTransaction BeginTransaction(this IDbContext dbContext)
        {
            return dbContext.Database.BeginTransaction();
        }

        public static IDbContextTransaction BeginTransaction(this IDbContext dbContext, IsolationLevel isolationLevel)
        {
            return dbContext.Database.BeginTransaction(isolationLevel);
        }
    }

    public static class ContextExtensions
    {
        //public static TContext GetContext<TContext>(this IDbContext dbContext) where TContext : class, new() => new TContext();

        //public static TContext GetContext<TContext>(this TContext context) where TContext : class, new() => new TContext();
    }
}