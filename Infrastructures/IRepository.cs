using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace Core.Data.Helper.Infrastructures
{
    public interface IRepository<TEntity>:IQueryable<TEntity> where TEntity : class
    {
        DbSet<TEntity> Entity { get; set; }

        int CountAll(params Expression<Func<TEntity, object>>[] includeProperties);

        int Count(Expression<Func<TEntity, bool>> where, params Expression<Func<TEntity, object>>[] includeProperties);
        int Count(params Expression<Func<TEntity, object>>[] includeProperties);

        Task<int> CountAsync(Expression<Func<TEntity, bool>> where, params Expression<Func<TEntity, object>>[] includeProperties);

        IQueryable<TEntity> AsQueryable();

        IQueryable<TEntity> AsQueryable(params Expression<Func<TEntity, object>>[] includeProperties);

        Task<IQueryable<TEntity>> AsQueryableAsync(params Expression<Func<TEntity, object>>[] includeProperties);

        IQueryable<TEntity> GetAll(params Expression<Func<TEntity, object>>[] includeProperties);

        Task<IQueryable<TEntity>> GetAllAsync(params Expression<Func<TEntity, object>>[] includeProperties);

        IQueryable<TEntity> Where(Expression<Func<TEntity, bool>> where,
            params Expression<Func<TEntity, object>>[] includeProperties);

        Task<IQueryable<TEntity>> WhereAsync(Expression<Func<TEntity, bool>> where,
            params Expression<Func<TEntity, object>>[] includeProperties);

        IQueryable<TEntity> Find(Expression<Func<TEntity, bool>> where,
            params Expression<Func<TEntity, object>>[] includeProperties);

        Task<IQueryable<TEntity>> FindAsync(Expression<Func<TEntity, bool>> where,
            params Expression<Func<TEntity, object>>[] includeProperties);

        TEntity Single();

        TEntity Single(Expression<Func<TEntity, bool>> where,
            params Expression<Func<TEntity, object>>[] includeProperties);

        Task<TEntity> SingleAsync(Expression<Func<TEntity, bool>> where,
            params Expression<Func<TEntity, object>>[] includeProperties);

        TEntity SingleOrDefault(Expression<Func<TEntity, bool>> where,
            params Expression<Func<TEntity, object>>[] includeProperties);

        Task<TEntity> SingleOrDefaultAsync(Expression<Func<TEntity, bool>> where,
            params Expression<Func<TEntity, object>>[] includeProperties);

        TEntity First(Expression<Func<TEntity, bool>> where,
            params Expression<Func<TEntity, object>>[] includeProperties);

        Task<TEntity> FirstAsync(Expression<Func<TEntity, bool>> where,
            params Expression<Func<TEntity, object>>[] includeProperties);

        TEntity FirstOrDefault(Expression<Func<TEntity, bool>> where,
            params Expression<Func<TEntity, object>>[] includeProperties);

        Task<TEntity> FirstOrDefaultAsync(Expression<Func<TEntity, bool>> where,
            params Expression<Func<TEntity, object>>[] includeProperties);

        bool Any(Expression<Func<TEntity, bool>> where, params Expression<Func<TEntity, object>>[] includeProperties);

        Task<bool> AnyAsync(Expression<Func<TEntity, bool>> where,
            params Expression<Func<TEntity, object>>[] includeProperties);

        IQueryable<TEntity> Pagination(int currentPage, int limit, out int rowsCount,
            params Expression<Func<TEntity, object>>[] includeProperties);

        Task<IQueryable<TEntity>> PaginationAsync(int currentPage, int limit, Func<int, int> rowsCount,
            params Expression<Func<TEntity, object>>[] includeProperties);

        IQueryable<TEntity> AsNoTracking();

        void Delete(TEntity entity);

        void Delete(Expression<Func<TEntity, bool>> match);

        Task DeleteAsync(TEntity entity);

        void Insert(TEntity entity);

        Task InsertAsync(TEntity entity);

        void Update(TEntity entity);

        TEntity Update(TEntity entity, Expression<Func<TEntity, bool>> match);

        Task UpdateAsync(TEntity entity);

        EntityEntry<TEntity> Entry(TEntity entity);

        IQueryable<TEntity> SqlQuery(string query, params object[] parameters);

        Task<IQueryable<TEntity>> SqlQueryAsync(string query, params object[] parameters);

        //IEnumerable<T> SqlQuery<T>(string query, params object[] parameters) where T : new();
        //Task<IEnumerable<T>> SqlQueryAsync<T>(string query, params object[] parameters) where T : new();

        int SaveChanges();

        Task<int> SaveChangesAsync();

        void InsertArray(ICollection<TEntity> entities);

        Task InsertArrayAsync(ICollection<TEntity> entities);
    }
}