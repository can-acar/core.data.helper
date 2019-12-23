using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace core.data.helper.infrastructures
{

    #pragma warning disable CS8603
    public abstract class BaseRepository<TContext> where TContext : class, IDisposable
    {
        private readonly IContextAdaptor<TContext> ContextAdaptor;

        private DbContext Context;

        protected BaseRepository(IContextAdaptor<TContext> contextAdaptor) { ContextAdaptor = contextAdaptor; }

        protected DbContext DbContext
        {
            get
            {
                if (Context != null)
                    return Context;

                return Context = ContextAdaptor.GetContext() as DbContext;
            }
        }
    }

    public abstract class Repository<TEntity, TContext> : BaseRepository<TContext>
    where TEntity : class, new()
    where TContext : class, IDisposable
    {
        protected Repository(IContextAdaptor<TContext> contextAdaptor) : base(contextAdaptor)
        {
            DbSet  = DbContext.Set<TEntity>();
            Entity = DbContext.Set<TEntity>();
        }

        private DbSet<TEntity> DbSet { get; }

        public DbSet<TEntity> Entity { get; set; }

        protected virtual IEnumerator<TEntity> GetEnumerator() { return DbSet.AsEnumerable().GetEnumerator(); }

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
        public virtual IQueryable<TEntity> AsQueryable() { return DbSet.AsQueryable(); }

        /// <summary>
        /// </summary>
        /// <returns></returns>
        public virtual async Task<IQueryable<TEntity>> AsQueryableAsync() { return await Task.FromResult(DbSet.AsQueryable()); }

        /// <summary>
        /// </summary>
        /// <param name="includeProperties"></param>
        /// <returns></returns>
        public virtual IQueryable<TEntity> AsQueryable(params Expression<Func<TEntity, object>>[] includeProperties) { return PerformInclusions(includeProperties).AsQueryable(); }

        /// <summary>
        /// </summary>
        /// <param name="includeProperties"></param>
        /// <returns></returns>
        public virtual Task<IQueryable<TEntity>> AsQueryableAsync(params Expression<Func<TEntity, object>>[] includeProperties)
        {
            return Task.Run(() => PerformInclusions(includeProperties).AsQueryable());
        }

        /// <summary>
        /// </summary>
        /// <param name="where"></param>
        /// <param name="includeProperties"></param>
        /// <returns></returns>
        public virtual int Count(Expression<Func<TEntity, bool>> where, params Expression<Func<TEntity, object>>[] includeProperties) { return PerformInclusions(includeProperties).Count(where); }

        /// <summary>
        /// </summary>
        /// <param name="includeProperties"></param>
        /// <returns></returns>
        public virtual int Count(params Expression<Func<TEntity, object>>[] includeProperties) { return PerformInclusions(includeProperties).Count(); }

        /// <summary>
        /// </summary>
        /// <param name="where"></param>
        /// <param name="includeProperties"></param>
        /// <returns></returns>
        public virtual Task<int> CountAsync(Expression<Func<TEntity, bool>> where, params Expression<Func<TEntity, object>>[] includeProperties)
        {
            return PerformInclusions(includeProperties).CountAsync(where);
        }

        /// <summary>
        /// </summary>
        /// <param name="includeProperties"></param>
        /// <returns></returns>
        public virtual Task<int> CountAsync(params Expression<Func<TEntity, object>>[] includeProperties) { return PerformInclusions(includeProperties).CountAsync(); }

        /// <summary>
        /// </summary>
        /// <param name="includeProperties"></param>
        /// <returns></returns>
        public virtual int CountAll(params Expression<Func<TEntity, object>>[] includeProperties) { return PerformInclusions(includeProperties).Count(); }

        /// <summary>
        /// </summary>
        /// <param name="where"></param>
        /// <param name="includeProperties"></param>
        /// <returns></returns>
        public virtual IQueryable<TEntity> Find(Expression<Func<TEntity, bool>> where, params Expression<Func<TEntity, object>>[] includeProperties)
        {
            return PerformInclusions(includeProperties).Where(where);
        }

        /// <summary>
        /// </summary>
        /// <param name="where"></param>
        /// <param name="includeProperties"></param>
        /// <returns></returns>
        public virtual Task<IQueryable<TEntity>> FindAsync(Expression<Func<TEntity, bool>> where, params Expression<Func<TEntity, object>>[] includeProperties)
        {
            return Task.Run(() => PerformInclusions(includeProperties).Where(where));
        }

        /// <summary>
        /// </summary>
        /// <param name="where"></param>
        /// <param name="includeProperties"></param>
        /// <returns></returns>
        public virtual TEntity First(Expression<Func<TEntity, bool>> where, params Expression<Func<TEntity, object>>[] includeProperties) { return PerformInclusions(includeProperties).First(where); }

        /// <summary>
        /// </summary>
        /// <param name="where"></param>
        /// <param name="includeProperties"></param>
        /// <returns></returns>
        public virtual Task<TEntity> FirstAsync(Expression<Func<TEntity, bool>> where, params Expression<Func<TEntity, object>>[] includeProperties)
        {
            return PerformInclusions(includeProperties).FirstAsync(where);
        }

        /// <summary>
        /// </summary>
        /// <param name="where"></param>
        /// <param name="includeProperties"></param>
        /// <returns></returns>
        public virtual TEntity FirstOrDefault(Expression<Func<TEntity, bool>> where, params Expression<Func<TEntity, object>>[] includeProperties)
        {
            return PerformInclusions(includeProperties).FirstOrDefault(where);
        }

        /// <summary>
        /// </summary>
        /// <param name="where"></param>
        /// <param name="includeProperties"></param>
        /// <returns></returns>
        public virtual Task<TEntity> FirstOrDefaultAsync(Expression<Func<TEntity, bool>> where, params Expression<Func<TEntity, object>>[] includeProperties)
        {
            return PerformInclusions(includeProperties).FirstOrDefaultAsync(where);
        }

        /// <summary>
        /// </summary>
        /// <param name="includeProperties"></param>
        /// <returns></returns>
        public virtual IQueryable<TEntity> GetAll(params Expression<Func<TEntity, object>>[] includeProperties) { return PerformInclusions(includeProperties).AsQueryable(); }

        /// <summary>
        /// </summary>
        /// <param name="includeProperties"></param>
        /// <returns></returns>
        public virtual Task<IQueryable<TEntity>> GetAllAsync(params Expression<Func<TEntity, object>>[] includeProperties)
        {
            return Task.Run(() => PerformInclusions(includeProperties).AsQueryable());
        }

        /// <summary>
        /// </summary>
        /// <returns></returns>
        public virtual TEntity Single() { return AsQueryable().Single(); }

        /// <summary>
        /// </summary>
        /// <param name="where"></param>
        /// <param name="includeProperties"></param>
        /// <returns></returns>
        public virtual TEntity Single(Expression<Func<TEntity, bool>> where, params Expression<Func<TEntity, object>>[] includeProperties)
        {
            return PerformInclusions(includeProperties).Single(where);
        }

        /// <summary>
        /// </summary>
        /// <param name="where"></param>
        /// <param name="includeProperties"></param>
        /// <returns></returns>
        public virtual Task<TEntity> SingleAsync(Expression<Func<TEntity, bool>> where, params Expression<Func<TEntity, object>>[] includeProperties)
        {
            return PerformInclusions(includeProperties).SingleAsync(where);
        }

        /// <summary>
        /// </summary>
        /// <param name="where"></param>
        /// <param name="includeProperties"></param>
        /// <returns></returns>
        public virtual TEntity SingleOrDefault(Expression<Func<TEntity, bool>> where, params Expression<Func<TEntity, object>>[] includeProperties)
        {
            return PerformInclusions(includeProperties).SingleOrDefault(where);
        }

        /// <summary>
        /// </summary>
        /// <param name="where"></param>
        /// <param name="includeProperties"></param>
        /// <returns></returns>
        public virtual Task<TEntity> SingleOrDefaultAsync(Expression<Func<TEntity, bool>> where, params Expression<Func<TEntity, object>>[] includeProperties)
        {
            return PerformInclusions(includeProperties).SingleOrDefaultAsync(where);
        }

        /// <summary>
        /// </summary>
        /// <param name="where"></param>
        /// <param name="includeProperties"></param>
        /// <returns></returns>
        public virtual IQueryable<TEntity> Where(Expression<Func<TEntity, bool>> where, params Expression<Func<TEntity, object>>[] includeProperties)
        {
            return PerformInclusions(includeProperties).Where(where);
        }

        /// <summary>
        /// </summary>
        /// <param name="where"></param>
        /// <param name="includeProperties"></param>
        /// <returns></returns>
        public virtual Task<IQueryable<TEntity>> WhereAsync(Expression<Func<TEntity, bool>> where, params Expression<Func<TEntity, object>>[] includeProperties)
        {
            return Task.Run(() => PerformInclusions(includeProperties).Where(where));
        }

        /// <summary>
        /// </summary>
        /// <param name="where"></param>
        /// <param name="includeProperties"></param>
        /// <returns></returns>
        public virtual bool Any(Expression<Func<TEntity, bool>> where, params Expression<Func<TEntity, object>>[] includeProperties) { return PerformInclusions(includeProperties).Any(where); }

        /// <summary>
        /// </summary>
        /// <param name="where"></param>
        /// <param name="includeProperties"></param>
        /// <returns></returns>
        public virtual Task<bool> AnyAsync(Expression<Func<TEntity, bool>> where, params Expression<Func<TEntity, object>>[] includeProperties)
        {
            return PerformInclusions(includeProperties).AnyAsync(where);
        }

        /// <summary>
        /// </summary>
        /// <param name="currentPage"></param>
        /// <param name="limit"></param>
        /// <param name="rowsCount"></param>
        /// <param name="includeProperties"></param>
        /// <returns></returns>
        public virtual IQueryable<TEntity> Pagination(int currentPage, int limit, out int rowsCount, params Expression<Func<TEntity, object>>[] includeProperties)
        {
            rowsCount = PerformInclusions(includeProperties).Count();

            return PerformInclusions(includeProperties).Skip((currentPage - 1) * limit).Take(limit).AsQueryable();
        }

        /// <summary>
        /// </summary>
        /// <param name="currentPage"></param>
        /// <param name="limit"></param>
        /// <param name="rowsCount"></param>
        /// <param name="includeProperties"></param>
        /// <returns></returns>
        public virtual Task<IQueryable<TEntity>> PaginationAsync(int currentPage, int limit, Func<int, int> rowsCount, params Expression<Func<TEntity, object>>[] includeProperties)
        {
            var RowsCount = PerformInclusions(includeProperties).Count();
            rowsCount(RowsCount);
            return Task.Run(() => PerformInclusions(includeProperties)
                                  .Skip((currentPage - 1) * limit).Take(limit).AsQueryable());
        }

        /// <summary>
        /// </summary>
        /// <returns></returns>
        public virtual IQueryable<TEntity> AsNoTracking()
        {
            // DbSet = DbContext.Set<TEntity>();
            return DbSet.AsNoTracking();
        }

        /// <summary>
        /// </summary>
        /// <param name="entity"></param>
        public virtual void Delete(TEntity entity)
        {
            //DbSet = DbContext.Set<TEntity>();

            if (DbContext.Entry(entity).State == EntityState.Detached) DbSet.Attach(entity);

            DbSet.Remove(entity);
        }

        public virtual void Delete(Expression<Func<TEntity, bool>> match)
        {
          

            var Items = DbSet.Where(match);
            foreach (var entity in Items)
            {
                if (DbContext.Entry(entity).State == EntityState.Detached) DbSet.Attach(entity);

                DbSet.Remove(entity);
            }
        }
        /// <summary>
        /// </summary>
        /// <param name="entity"></param>
        public virtual async Task DeleteAsync(TEntity entity)
        {
            //DbSet = DbContext.Set<TEntity>();

            if (DbContext.Entry(entity).State == EntityState.Detached) DbSet.Attach(entity);

            await Task.Run(() => DbSet.Remove(entity));
        }

        /// <summary>
        /// </summary>
        /// <param name="entity"></param>
        public virtual void Insert(TEntity entity) { DbSet.Add(entity); }

        public virtual void InsertArray(ICollection<TEntity> entities) { DbSet.AddRange(entities); }

        public virtual async Task InsertArrayAsync(ICollection<TEntity> entities) { await Task.Run(() => DbSet.AddRange(entities)); }

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
            DbContext.Entry(entity).State = EntityState.Modified;
            DbSet.Update(entity);
        }

        /// <summary>
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        public virtual async Task UpdateAsync(TEntity entity)
        {
            // DbSet = DbContext.Set<TEntity>();
            DbContext.Entry(entity).State = EntityState.Modified;
            await Task.Run(() => DbSet.Update(entity));
        }

        /// <summary>
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="match"></param>
        /// <returns></returns>
        public virtual TEntity Update(TEntity entity, Expression<Func<TEntity, bool>> match)
        {
            if (entity == null)
                return null;

            var Existing = DbSet.SingleOrDefault(match);

            if (Existing == null) return new TEntity();

            DbContext.Entry(entity).State = EntityState.Modified;
            DbSet.Update(entity);
            return Existing;
        }

        /// <summary>
        /// </summary>
        /// <returns></returns>
        public int SaveChanges() { return DbContext.SaveChanges(); }

        /// <summary>
        /// </summary>
        /// <returns></returns>
        public Task<int> SaveChangesAsync() { return DbContext.SaveChangesAsync(); }

        /// <summary>
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        public EntityEntry<TEntity> Entry(TEntity entity) { return DbContext.Entry(entity); }

        /// <summary>
        /// </summary>
        /// <param name="query"></param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        public virtual IEnumerable<TEntity> SqlQuery(string query, params object[] parameters) { return DbSet.FromSqlRaw(query, parameters); }

        public virtual Task<IEnumerable<TEntity>> SqlQueryAsync(string query, params object[] parameters) { return Task.Run(() => SqlQuery(query, parameters)); }

        /// <summary>
        /// </summary>
        /// <param name="query"></param>
        /// <param name="objects"></param>
        public virtual int ExecuteSqlCommand(string query, params object[] objects) { return DbContext.Database.ExecuteSqlRaw(query, objects); }
    }

}