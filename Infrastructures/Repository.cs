using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace Core.Data.Helper.Infrastructures
{
#pragma warning disable CS8603
    public abstract class BaseRepository<TEntity> where TEntity : class
    {
        public abstract DbSet<TEntity> Entity { get; set; }

        protected BaseRepository(DbContext context)
        {
            Context = context;
        }

        protected DbContext Context { get; }
    }

    public class Repository<TEntity> : BaseRepository<TEntity>, IRepository<TEntity> where TEntity : class, new()
    //where TContext : class, IDisposable
    {
        private readonly DbSet<TEntity> DbSet;

        public override DbSet<TEntity> Entity { get; set; }

        protected Repository(DbContext context) : base(context)
        {
            DbSet = context.Set<TEntity>();
            Entity = context.Set<TEntity>();
        }


        protected virtual IEnumerator<TEntity> GetEnumerator() =>
            DbSet.AsEnumerable()
                .GetEnumerator();

        /// <summary>
        /// </summary>
        /// <param name="includeProperties"></param>
        /// <returns></returns>
        private IQueryable<TEntity> PerformInclusions(params Expression<Func<TEntity, object>>[] includeProperties)
        {
            return includeProperties.Aggregate<Expression<Func<TEntity, object>>, IQueryable<TEntity>>(DbSet, (current, includeProperty) => current.Include(includeProperty));
        }

        ///// <summary>
        ///// 
        ///// </summary>
        ///// <param name="includeProperties"></param>
        ///// <returns></returns>
        //public IQueryable<TEntity> Include(params Expression<Func<TEntity, object>>[] includeProperties)
        //{
        //    return includeProperties.Aggregate<Expression<Func<TEntity, object>>, IQueryable<TEntity>>(DbSet, (current, includeProperty) => current.Include(includeProperty));
        //}
        ////public IIncludableQueryable<TEntity, TEntity> Include(Expression<Func<TEntity, TEntity>> includeProperties) 
        ////{
        ////    return DbSet.Include(includeProperties);
        ////}

        /// <summary>
        /// </summary>
        /// <returns>
        ///     @IQueryable
        /// </returns>
        public virtual IQueryable<TEntity> AsQueryable() => DbSet.AsQueryable();

        /// <summary>
        /// </summary>
        /// <returns></returns>
        public virtual async Task<IQueryable<TEntity>> AsQueryableAsync() => await Task.FromResult(DbSet.AsQueryable());

        /// <summary>
        /// </summary>
        /// <param name="includeProperties"></param>
        /// <returns></returns>
        public virtual IQueryable<TEntity> AsQueryable(params Expression<Func<TEntity, object>>[] includeProperties) =>
            PerformInclusions(includeProperties)
                .AsQueryable();

        /// <summary>
        /// </summary>
        /// <param name="includeProperties"></param>
        /// <returns></returns>
        public virtual Task<IQueryable<TEntity>> AsQueryableAsync(params Expression<Func<TEntity, object>>[] includeProperties)
        {
            return Task.Run(() => PerformInclusions(includeProperties)
                .AsQueryable());
        }

        /// <summary>
        /// </summary>
        /// <param name="where"></param>
        /// <param name="includeProperties"></param>
        /// <returns></returns>
        public virtual int Count(Expression<Func<TEntity, bool>> where, params Expression<Func<TEntity, object>>[] includeProperties) =>
            PerformInclusions(includeProperties)
                .Count(where);

        /// <summary>
        /// </summary>
        /// <param name="includeProperties"></param>
        /// <returns></returns>
        public virtual int Count(params Expression<Func<TEntity, object>>[] includeProperties) =>
            PerformInclusions(includeProperties)
                .Count();

        /// <summary>
        /// </summary>
        /// <param name="where"></param>
        /// <param name="includeProperties"></param>
        /// <returns></returns>
        public virtual Task<int> CountAsync(Expression<Func<TEntity, bool>> where, params Expression<Func<TEntity, object>>[] includeProperties) =>
            PerformInclusions(includeProperties)
                .CountAsync(where);

        /// <summary>
        /// </summary>
        /// <param name="includeProperties"></param>
        /// <returns></returns>
        public virtual Task<int> CountAsync(params Expression<Func<TEntity, object>>[] includeProperties) =>
            PerformInclusions(includeProperties)
                .CountAsync();


        /// <summary>
        /// </summary>
        /// <param name="includeProperties"></param>
        /// <returns></returns>
        public virtual int CountAll(params Expression<Func<TEntity, object>>[] includeProperties) =>
            PerformInclusions(includeProperties)
                .Count();

        /// <summary>
        /// </summary>
        /// <param name="where"></param>
        /// <param name="includeProperties"></param>
        /// <returns></returns>
        public virtual IQueryable<TEntity> Find(Expression<Func<TEntity, bool>> where, params Expression<Func<TEntity, object>>[] includeProperties) =>
            PerformInclusions(includeProperties)
                .Where(where);

        /// <summary>
        /// </summary>
        /// <param name="where"></param>
        /// <param name="includeProperties"></param>
        /// <returns></returns>
        public virtual Task<IQueryable<TEntity>> FindAsync(Expression<Func<TEntity, bool>> where,
            params Expression<Func<TEntity, object>>[] includeProperties)
        {
            return Task.Run(() => PerformInclusions(includeProperties)
                .Where(where));
        }

        /// <summary>
        /// </summary>
        /// <param name="where"></param>
        /// <param name="includeProperties"></param>
        /// <returns></returns>
        public virtual TEntity First(Expression<Func<TEntity, bool>> where, params Expression<Func<TEntity, object>>[] includeProperties) =>
            PerformInclusions(includeProperties)
                .First(where);

        /// <summary>
        /// </summary>
        /// <param name="where"></param>
        /// <param name="includeProperties"></param>
        /// <returns></returns>
        public virtual Task<TEntity> FirstAsync(Expression<Func<TEntity, bool>> where, params Expression<Func<TEntity, object>>[] includeProperties) =>
            PerformInclusions(includeProperties)
                .FirstAsync(where);

        /// <summary>
        /// </summary>
        /// <param name="where"></param>
        /// <param name="includeProperties"></param>
        /// <returns></returns>
        public virtual TEntity FirstOrDefault(Expression<Func<TEntity, bool>> where, params Expression<Func<TEntity, object>>[] includeProperties) =>
            PerformInclusions(includeProperties)
                .FirstOrDefault(where);

        /// <summary>
        /// </summary>
        /// <param name="where"></param>
        /// <param name="includeProperties"></param>
        /// <returns></returns>
        public virtual Task<TEntity> FirstOrDefaultAsync(Expression<Func<TEntity, bool>> where, params Expression<Func<TEntity, object>>[] includeProperties) =>
            PerformInclusions(includeProperties)
                .FirstOrDefaultAsync(where);

        /// <summary>
        /// </summary>
        /// <param name="includeProperties"></param>
        /// <returns></returns>
        public virtual IQueryable<TEntity> GetAll(params Expression<Func<TEntity, object>>[] includeProperties) =>
            PerformInclusions(includeProperties)
                .AsQueryable();

        /// <summary>
        /// </summary>
        /// <param name="includeProperties"></param>
        /// <returns></returns>
        public virtual Task<IQueryable<TEntity>> GetAllAsync(params Expression<Func<TEntity, object>>[] includeProperties)
        {
            return Task.Run(() => PerformInclusions(includeProperties)
                .AsQueryable());
        }

        /// <summary>
        /// </summary>
        /// <returns></returns>
        public virtual TEntity Single() =>
            AsQueryable()
                .Single();

        /// <summary>
        /// </summary>
        /// <param name="where"></param>
        /// <param name="includeProperties"></param>
        /// <returns></returns>
        public virtual TEntity Single(Expression<Func<TEntity, bool>> where, params Expression<Func<TEntity, object>>[] includeProperties) =>
            PerformInclusions(includeProperties)
                .Single(where);

        /// <summary>
        /// </summary>
        /// <param name="where"></param>
        /// <param name="includeProperties"></param>
        /// <returns></returns>
        public virtual Task<TEntity> SingleAsync(Expression<Func<TEntity, bool>> where, params Expression<Func<TEntity, object>>[] includeProperties) =>
            PerformInclusions(includeProperties)
                .SingleAsync(where);

        /// <summary>
        /// </summary>
        /// <param name="where"></param>
        /// <param name="includeProperties"></param>
        /// <returns></returns>
        public virtual TEntity SingleOrDefault(Expression<Func<TEntity, bool>> where, params Expression<Func<TEntity, object>>[] includeProperties) =>
            PerformInclusions(includeProperties)
                .SingleOrDefault(where);

        /// <summary>
        /// </summary>
        /// <param name="where"></param>
        /// <param name="includeProperties"></param>
        /// <returns></returns>
        public virtual Task<TEntity> SingleOrDefaultAsync(Expression<Func<TEntity, bool>> where, params Expression<Func<TEntity, object>>[] includeProperties) =>
            PerformInclusions(includeProperties)
                .SingleOrDefaultAsync(where);

        /// <summary>
        /// </summary>
        /// <param name="where"></param>
        /// <param name="includeProperties"></param>
        /// <returns></returns>
        public virtual IQueryable<TEntity> Where(Expression<Func<TEntity, bool>> where, params Expression<Func<TEntity, object>>[] includeProperties) =>
            PerformInclusions(includeProperties)
                .Where(where);

        /// <summary>
        /// </summary>
        /// <param name="where"></param>
        /// <param name="includeProperties"></param>
        /// <returns></returns>
        public virtual Task<IQueryable<TEntity>> WhereAsync(Expression<Func<TEntity, bool>> where,
            params Expression<Func<TEntity, object>>[] includeProperties)
        {
            return Task.Run(() => PerformInclusions(includeProperties)
                .Where(where));
        }

        /// <summary>
        /// </summary>
        /// <param name="where"></param>
        /// <param name="includeProperties"></param>
        /// <returns></returns>
        public virtual bool Any(Expression<Func<TEntity, bool>> where, params Expression<Func<TEntity, object>>[] includeProperties) =>
            PerformInclusions(includeProperties)
                .Any(where);

        /// <summary>
        /// </summary>
        /// <param name="where"></param>
        /// <param name="includeProperties"></param>
        /// <returns></returns>
        public virtual Task<bool> AnyAsync(Expression<Func<TEntity, bool>> where, params Expression<Func<TEntity, object>>[] includeProperties) =>
            PerformInclusions(includeProperties)
                .AnyAsync(where);

        /// <summary>
        /// </summary>
        /// <param name="currentPage"></param>
        /// <param name="limit"></param>
        /// <param name="rowsCount"></param>
        /// <param name="includeProperties"></param>
        /// <returns></returns>
        public virtual IQueryable<TEntity> Pagination(int currentPage, int limit, out int rowsCount,
            params Expression<Func<TEntity, object>>[] includeProperties)
        {
            rowsCount = PerformInclusions(includeProperties)
                .Count();

            return PerformInclusions(includeProperties)
                .Skip((currentPage - 1) * limit)
                .Take(limit)
                .AsQueryable();
        }

        /// <summary>
        /// </summary>
        /// <param name="currentPage"></param>
        /// <param name="limit"></param>
        /// <param name="rowsCount"></param>
        /// <param name="includeProperties"></param>
        /// <returns></returns>
        public virtual Task<IQueryable<TEntity>> PaginationAsync(int currentPage, int limit, Func<int, int> rowsCount,
            params Expression<Func<TEntity, object>>[] includeProperties)
        {
            var Count = PerformInclusions(includeProperties)
                .Count();

            rowsCount(Count);

            return Task.Run(() => PerformInclusions(includeProperties)
                .Skip((currentPage - 1) * limit)
                .Take(limit)
                .AsQueryable());
        }

        /// <summary>
        /// </summary>
        /// <returns></returns>
        // DbSet = DbContext.Set<TEntity>();
        public virtual IQueryable<TEntity> AsNoTracking() => DbSet.AsNoTracking();

        /// <summary>
        /// </summary>
        /// <param name="entity"></param>
        public virtual void Delete(TEntity entity)
        {
            //DbSet = DbContext.Set<TEntity>();

            if (Context.Entry(entity)
                .State == EntityState.Detached)
                DbSet.Attach(entity);

            DbSet.Remove(entity);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="match"></param>
        public virtual void Delete(Expression<Func<TEntity, bool>> match)
        {
            var Items = DbSet.Where(match).AsQueryable();

            foreach (var Item in Items)
            {
                if (Context.Entry(Item).State == EntityState.Detached)
                    DbSet.Attach(Item);

                DbSet.Remove(Item);
            }
        }

        /// <summary>
        /// </summary>
        /// <param name="entity"></param>
        public virtual async Task DeleteAsync(TEntity entity)
        {
            //DbSet = DbContext.Set<TEntity>();

            if (Context.Entry(entity).State == EntityState.Detached)
                DbSet.Attach(entity);

            await Task.Run(() => DbSet.Remove(entity));
        }

        /// <summary>
        /// </summary>
        /// <param name="entity"></param>
        public virtual void Insert(TEntity entity)
        {
            DbSet.Add(entity);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="entities"></param>
        public virtual void InsertArray(ICollection<TEntity> entities)
        {
            DbSet.AddRange(entities);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="entities"></param>
        /// <returns></returns>
        public virtual async Task InsertArrayAsync(ICollection<TEntity> entities)
        {
            await Task.Run(() => DbSet.AddRange(entities));
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        public virtual async Task InsertAsync(TEntity entity)
        {
            //DbSet = DbContext.Set<TEntity>();
            await Task.Run(() => DbSet.Add(entity));
        }

        /// <summary>
        /// </summary>
        /// <param name="entity"></param>
        public virtual void Update(TEntity entity)
        {
            // DbSet = DbContext.Set<TEntity>();
            Context.Entry(entity)
                .State = EntityState.Modified;

            DbSet.Update(entity);
        }

        /// <summary>
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        public virtual async Task UpdateAsync(TEntity entity)
        {
            // DbSet = DbContext.Set<TEntity>();
            Context.Entry(entity)
                .State = EntityState.Modified;

            await Task.Run(() => DbSet.Update(entity));
        }

        /// <summary>
        /// </summary>
        /// <param name="entity"></param>
        /// <param name="match"></param>
        /// <returns></returns>
        public virtual TEntity Update(TEntity entity, Expression<Func<TEntity, bool>> match)
        {
            if (entity == null)
                return null;

            var Existing = DbSet.SingleOrDefault(match);

            if (Existing == null)
                return new TEntity();

            Context.Entry(entity).State = EntityState.Modified;

            DbSet.Update(entity);

            return Existing;
        }

        /// <summary>
        /// </summary>
        /// <returns></returns>
        public int SaveChanges() => Context.SaveChanges();

        /// <summary>
        /// </summary>
        /// <returns></returns>
        public Task<int> SaveChangesAsync() => Context.SaveChangesAsync();

        /// <summary>
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        public EntityEntry<TEntity> Entry(TEntity entity) => Context.Entry(entity);

        /// <summary>
        /// </summary>
        /// <param name="query"></param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        public virtual IQueryable<TEntity> SqlQuery(string query, params object[] parameters) => DbSet.FromSqlRaw(query, parameters);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="query"></param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        public virtual Task<IQueryable<TEntity>> SqlQueryAsync(string query, params object[] parameters)
        {
            return Task.FromResult(DbSet.FromSqlRaw(query, parameters));
        }
    }
}