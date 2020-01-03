using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using core.data.helper.infrastructures;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;

namespace core.data.helper.extensions
{

    #pragma warning disable CS8603
    public static class QueryableExtensions
    {
        public static ProjectionExpression<TSource> Project<TSource>(this IQueryable<TSource> source) { return new ProjectionExpression<TSource>(source); }

        public static IQueryable<TResult> Select<TEntity, TResult>(this IRepository<TEntity> source, Expression<Func<TEntity, TResult>> query) where TEntity : class
        {
            //            MethodInfo MethodInfo = new Func<IQueryable<object>, Expression<Func<object, object>>, IQueryable<object>>(Queryable.Select<object, object>).GetMethodInfo()
            //                .GetGenericMethodDefinition();
            //
            //            return source.Entity.AsQueryable().Provider.CreateQuery<TResult>((Expression)
            //                Expression.Call((Expression) null, MethodInfo.MakeGenericMethod(typeof(TEntity), typeof(TResult)), source.Entity.AsQueryable().Expression,
            //                    (Expression) Expression.Quote((Expression) query)));


            return source.Entity.Select(query).AsQueryable();
        }

        public static IQueryable<TEntity> Include<TEntity, TProperty>(this IRepository<TEntity> source,
                                                                      params Expression<Func<TEntity, TProperty>>[] navigationPropertyPath)
        where TEntity : class
        {
            return navigationPropertyPath.Aggregate<Expression<Func<TEntity, TProperty>>, IQueryable<TEntity>>(source.Entity,
                                                                                                               (entities, expression) => entities.Include(expression));
            // return source.Entity.Include(navigationPropertyPath);
        }

        public static IIncludableQueryable<TEntity, TProperty> Include<TEntity, TProperty>(this IRepository<TEntity> source,
                                                                                           Expression<Func<TEntity, TProperty>> navigationPropertyPath)
        where TEntity : class
        {
            return source.Entity.Include(navigationPropertyPath);
        }

        public static IQueryable<TResult> InnerJoin<TSource, TInner, TKey, TResult>(this IRepository<TSource> source, IRepository<TInner> other, Func<TSource, TKey> func,
                                                                                    Func<TInner, TKey> innerkey,
                                                                                    Func<TSource, TInner, TResult> res) where TSource : class where TInner : class
        {
            return
                from F in source.AsQueryable()
                join B in other.AsQueryable() on func.Invoke(F) equals innerkey.Invoke(B) into G
                from Result in G
                select res.Invoke(F, Result);
        }

        public static IQueryable<TResult> LeftOuterJoin<TSource, TInner, TKey, TResult>(this IRepository<TSource> source,
                                                                                        IRepository<TInner> other,
                                                                                        Func<TSource, TKey> func,
                                                                                        Func<TInner, TKey> innerkey,
                                                                                        Func<TSource, TInner, TResult> res) where TSource : class where TInner : class
        {
            return
                from F in source.AsQueryable()
                join B in other.AsQueryable() on func.Invoke(F) equals innerkey.Invoke(B) into G
                from Result in G.DefaultIfEmpty()
                select res.Invoke(F, Result);
        }

        public static IQueryable<TResult> LeftJoin<TOuter, TInner, TKey, TResult>(this IRepository<TOuter> outer,
                                                                                  IRepository<TInner> inner,
                                                                                  Func<TOuter, TKey> outerKeySelector,
                                                                                  Func<TInner, TKey> innerKeySelector,
                                                                                  Func<TOuter, TInner, TResult>
                                                                                      resultSelector,
                                                                                  IEqualityComparer<TKey> comparer) where TOuter : class where TInner : class
        {
            return outer.AsQueryable().AsEnumerable().GroupJoin(inner.AsQueryable(),
                                                                outerKeySelector,
                                                                innerKeySelector,
                                                                (o, ei) => ei
                                                                           .Select(i => resultSelector(o, i))
                                                                           .DefaultIfEmpty(resultSelector(o, default)), comparer)
                        .SelectMany(oi => oi).AsQueryable();
        }

        public static IQueryable<TResult> LeftJoin<TOuter, TInner, TKey, TResult>(this IRepository<TOuter> outer,
                                                                                  IRepository<TInner> inner,
                                                                                  Func<TOuter, TKey> outerKeySelector,
                                                                                  Func<TInner, TKey> innerKeySelector,
                                                                                  Func<TOuter, TInner, TResult>
                                                                                      resultSelector) where TInner : class where TOuter : class
        {
            return outer.LeftJoin(inner, outerKeySelector, innerKeySelector, resultSelector, default);
        }

        public static IQueryable<TEntity> Pagination<TEntity>(this IRepository<TEntity> source, int currentPage,
                                                              int limit, out int rowCount) where TEntity : class
        {
            rowCount = source.Count();

            return source.AsQueryable()
                         .Skip((currentPage - 1) * limit)
                         .Take(limit);
        }

        public static async Task<TEntity[]> PaginationAsync<TEntity>(this IQueryable<TEntity> source, int currentPage,
                                                                     int limit) where TEntity : class
        {
            return await source.AsNoTracking()
                               .Skip((currentPage - 1) * limit)
                               .Take(limit)
                               .ToArrayAsync();
        }

        public static Task<IQueryable<TSource>> WhereAsync<TSource>(this IQueryable<TSource> source,
                                                                    Expression<Func<TSource, bool>> predicate) where TSource : class
        {
            return Task.Run(() => source.Where(predicate));
        }
    }

    #pragma warning restore CS8603
    public class ProjectionExpression<TSource>
    {
        // ReSharper disable once StaticMemberInGenericType
        private static readonly Dictionary<string, Expression> ExpressionCache = new Dictionary<string, Expression>();

        private readonly IQueryable<TSource> Source;

        public ProjectionExpression(IQueryable<TSource> source) { Source = source; }

        public IQueryable<TDest> To<TDest>()
        {
            var QueryExpression = GetCachedExpression<TDest>() ?? BuildExpression<TDest>();

            return Source.Select(QueryExpression);
        }
        #pragma warning disable CS8603
        private static Expression<Func<TSource, TDest>> GetCachedExpression<TDest>()
        {
            var Key = GetCacheKey<TDest>();

            return ExpressionCache.ContainsKey(Key)
                ? ExpressionCache[Key] as Expression<Func<TSource, TDest>>
                : null;
        }

        private static Expression<Func<TSource, TDest>> BuildExpression<TDest>()
        {
            var SourceProperties      = typeof(TSource).GetProperties();
            var DestinationProperties = typeof(TDest).GetProperties().Where(dest => dest.CanWrite);
            var ParameterExpression   = Expression.Parameter(typeof(TSource), "src");

            var Bindings = DestinationProperties
                           .Select(destinationProperty =>
                                       BuildBinding(ParameterExpression, destinationProperty, SourceProperties))
                           .Where(binding => binding != null);

            var ExpressionLambda = Expression.Lambda<Func<TSource, TDest>>(Expression.MemberInit(Expression.New(typeof(TDest)), Bindings),
                                                                           ParameterExpression);

            Bindings = DestinationProperties
                       .Select(destinationProperty =>
                                   BuildBinding(ParameterExpression, destinationProperty, SourceProperties))
                       .Where(binding => binding != null);

            ExpressionLambda = Expression.Lambda<Func<TSource, TDest>>(Expression.MemberInit(Expression.New(typeof(TDest)), Bindings),
                                                                       ParameterExpression);

            var Key = GetCacheKey<TDest>();

            ExpressionCache.Add(Key, ExpressionLambda);

            return ExpressionLambda;
        }
        #pragma warning restore CS8603
        private static MemberAssignment BuildBinding(Expression parameterExpression, MemberInfo destinationProperty,
                                                     IEnumerable<PropertyInfo> sourceProperties)
        {
            IEnumerable<PropertyInfo> PropertyInfos = sourceProperties as PropertyInfo[] ?? sourceProperties.ToArray();

            var SourceProperty =
                PropertyInfos.FirstOrDefault(src => src.Name == destinationProperty.Name);

            if (SourceProperty != null)
                return Expression.Bind(destinationProperty, Expression.Property(parameterExpression, SourceProperty));

            var PropertyNames = SplitCamelCase(destinationProperty.Name);
            #pragma warning disable CS8603
            if (PropertyNames.Length != 2)
                return null;

            PropertyNames = SplitCamelCase(destinationProperty.Name);
            #pragma warning disable CS8603
            if (PropertyNames.Length != 2) return null;
            {
                SourceProperty = PropertyInfos.FirstOrDefault(src => src.Name == PropertyNames[0]);
                #pragma warning disable CS8603
                if (SourceProperty == null) return null;
                {
                    var SourceChildProperty = SourceProperty.PropertyType.GetProperties()
                                                            .FirstOrDefault(src => src.Name == PropertyNames[1]);

                    if (SourceProperty != null)
                        return Expression.Bind(destinationProperty,
                                               Expression
                                                   .Property(Expression.Property(parameterExpression, SourceProperty),
                                                             SourceChildProperty));
                }
            }
            #pragma warning disable CS8603
            return null;
        }

        private static string GetCacheKey<TDest>() { return string.Concat(typeof(TSource).FullName, typeof(TDest).FullName); }

        private static string[] SplitCamelCase(string input) { return Regex.Replace(input, "([A-Z])", " $1", RegexOptions.Compiled).Trim().Split(' '); }
    }

}