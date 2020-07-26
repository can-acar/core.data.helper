using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading.Tasks;
using Core.Data.Helper.Infrastructures;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;

namespace Core.Data.Helper.Extensions
{
    public static class QueryableExtensions
    {
        public static ProjectionExpression<TSource> Project<TSource>(this IQueryable<TSource> source) => new ProjectionExpression<TSource>(source);

        public static IQueryable<TResult> Select<TEntity, TResult>(this IRepository<TEntity> source,
                                                                   Expression<Func<TEntity, TResult>> query) where TEntity : class
        {
            return source.Entity.Select(query)
                         .AsQueryable();
        }

        public static IQueryable<TEntity> Include<TEntity, TProperty>(this IRepository<TEntity> source,
                                                                      params Expression<Func<TEntity, TProperty>>[] navigationPropertyPath)
        where TEntity : class
        {
            return navigationPropertyPath.Aggregate<Expression<Func<TEntity, TProperty>>, IQueryable<TEntity>>(
                                                                                                               source.Entity,
                                                                                                               (entities, expression) => entities.Include(expression));
        }

        public static IIncludableQueryable<TEntity, TProperty> Include<TEntity, TProperty>(this IRepository<TEntity> source,
                                                                                           Expression<Func<TEntity, TProperty>> navigationPropertyPath)
        where TEntity : class
        {
            return source.Entity.Include(navigationPropertyPath);
        }

        public static IQueryable<TResult> InnerJoin<TSource, TInner, TKey, TResult>(this IRepository<TSource> source,
                                                                                    IRepository<TInner> other, Func<TSource, TKey> func,
                                                                                    Func<TInner, TKey> innerkey,
                                                                                    Func<TSource, TInner, TResult> res) where TSource : class where TInner : class
        {
            return from F in source.AsQueryable()
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
            return from F in source.AsQueryable()
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
            return outer.AsQueryable()
                        .AsEnumerable()
                        .GroupJoin(inner.AsQueryable(),
                                   outerKeySelector,
                                   innerKeySelector,
                                   (o, ei) => ei
                                              .Select(i => resultSelector(o, i))
                                              .DefaultIfEmpty(resultSelector(o, default)), comparer)
                        .SelectMany(oi => oi)
                        .AsQueryable();
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

            return source.Entity
                         .Skip((currentPage - 1) * limit)
                         .Take(limit).AsQueryable();
        }

        public static async Task<TEntity[]> PaginationAsync<TEntity>(this IQueryable<TEntity> source,
                                                                     int currentPage,
                                                                     int limit) where TEntity : class
        {
            return await source
                         .Skip((currentPage - 1) * limit)
                         .Take(limit)
                         .ToArrayAsync();
        }

        public static Task<IQueryable<TSource>> WhereAsync<TSource>(this IQueryable<TSource> source,
                                                                    Expression<Func<TSource, bool>> predicate) where TSource : class
        {
            return Task.Run(() => source.Where(predicate));
        }

        public static IQueryable<T> Select<T>(this IQueryable<T> source, string columnName)
        {
            if (string.IsNullOrEmpty(columnName)) { return source; }

            var Parameter = Expression.Parameter(source.ElementType, "p");
            var Property  = Expression.Property(Parameter, columnName);
            var Lambda    = Expression.Lambda(Property, Parameter);

            //string methodName = isAscending ? "OrderBy" : "OrderByDescending";  
            const string methodName = "Select"; // : "OrderByDescending";  

            Expression MethodCallExpression = Expression.Call(typeof(Queryable), methodName,
                                                              new[] {source.ElementType, Property.Type},
                                                              source.Expression, Expression.Quote(Lambda));

            return source.Provider.CreateQuery<T>(MethodCallExpression)
                         .AsQueryable();
        }

        private static class PropertyAccessorCache<T> where T : class
        {
            private static readonly IDictionary<string, LambdaExpression> Cache;

            static PropertyAccessorCache()
            {
                var Storage = new Dictionary<string, LambdaExpression>();

                var T         = typeof(T);
                var Parameter = Expression.Parameter(T, "p");

                foreach (var Property in T.GetProperties(BindingFlags.Public | BindingFlags.Instance))
                {
                    var PropertyAccess   = Expression.MakeMemberAccess(Parameter, Property);
                    var LambdaExpression = Expression.Lambda(PropertyAccess, Parameter);
                    Storage[Property.Name] = LambdaExpression;
                }

                Cache = Storage;
            }

            public static LambdaExpression Get(string propertyName)
            {
                return Cache.TryGetValue(propertyName, out var Result)
                    ? Result
                    : null;
            }
        }

        public static IQueryable<T> Filter<T>(this IQueryable<T> source, string propertyName, object propertyValue) where T : class
        {
            var Param       = Expression.Parameter(typeof(T), typeof(T).Name.ToLower());
            var Property    = Expression.Property(Param, propertyName);
            var SearchValue = Convert.ChangeType(propertyValue, Property.Type);


            Expression MatchExpression = Property;

            if (MatchExpression.Type != typeof(string))
            {
                MatchExpression = Expression.Convert(MatchExpression, typeof(object));
                MatchExpression = Expression.Convert(MatchExpression, typeof(string));
            }

            var Pattern = Expression.Constant($"%{SearchValue}%");

            var Expr = Expression.Call(typeof(DbFunctionsExtensions), "Like", Type.EmptyTypes,
                                       Expression.Constant(EF.Functions), MatchExpression, Pattern);

            return source.Where(Expression.Lambda<Func<T, bool>>(Expr, Param))
                         .AsQueryable();
        }

        public static IQueryable<T> Where<T>(this IQueryable<T> source, string propertyName, object propertyValue, out bool success) where T : class
        {
            success = false;
            var Mba = PropertyAccessorCache<T>.Get(propertyName);

            if (Mba == null)
                return source;


            object Value;

            try { Value = Convert.ChangeType(propertyValue, Mba.ReturnType); }
            catch (SystemException Ex) when (Ex is InvalidCastException ||
                                             Ex is FormatException ||
                                             Ex is OverflowException ||
                                             Ex is ArgumentNullException) { return source; }


            var Eqe = Expression.Equal(Mba.Body, Expression.Constant(Value, Mba.ReturnType));

            var QueryExpr = Expression.Lambda(Eqe, Mba.Parameters[0]);


            success = true;

            var ResultExpression = Expression.Call(null,
                                                   GetMethodInfo<IQueryable<T>,
                                                       Expression<Func<T, bool>>,
                                                       IQueryable<T>>(Queryable.Where),
                                                   new[] {source.Expression, Expression.Quote(QueryExpr)});

            return source.Provider.CreateQuery<T>(ResultExpression);
        }

        private static MethodInfo GetMethodInfo<T1, T2, T3>(Func<T1, T2, T3> f) { return f.Method; }

        public static IQueryable<T> SortBy<T>(this IQueryable<T> source, string columnName)
        {
            if (string.IsNullOrEmpty(columnName)) return source;

            var Parameter = Expression.Parameter(source.ElementType, "");
            var Property  = Expression.Property(Parameter, columnName);
            var Lambda    = Expression.Lambda(Property, Parameter);

            //string methodName = isAscending ? "OrderBy" : "OrderByDescending";  
            var MethodName = "OrderBy"; // : "OrderByDescending";  

            Expression MethodCallExpression = Expression.Call(typeof(Queryable), MethodName,
                                                              new[] {source.ElementType, Property.Type},
                                                              source.Expression, Expression.Quote(Lambda));

            return source.Provider.CreateQuery<T>(MethodCallExpression)
                         .AsQueryable();
        }

        public static IQueryable<T> SortBy<T>(this IQueryable<T> source, IEnumerable<string> columnNames)
        {
            if (source == null)
                throw new ArgumentNullException(nameof(source));


            var QueryExpr = source.Expression;
            var Parameter = Expression.Parameter(source.ElementType, "");

            var MethodName = "OrderBy";

            foreach (var ColumnName in columnNames)
            {
                var Property = Expression.Property(Parameter, ColumnName);
                var Lambda   = Expression.Lambda(Property, Parameter);

                QueryExpr = Expression.Call(typeof(Queryable), MethodName,
                                            new[] {source.ElementType, Property.Type},
                                            QueryExpr, Expression.Quote(Lambda));

                MethodName = "ThenBy";
            }


            return source.Provider.CreateQuery<T>(QueryExpr)
                         .AsQueryable();
        }

        public static IQueryable<T> SortBy<T>(this IQueryable<T> source, IList<IDictionary<string, string>> orders)
        {
            if (source == null)
                throw new ArgumentNullException(nameof(source));


            var EntityType = source.ElementType;

            var EntityParameter = Expression.Parameter(EntityType, "p");


            var OrderType = orders[0]["orderType"];

            var MethodName = OrderType == "asc"
                ? "OrderBy"
                : "OrderByDescending";


            var OrderBy = orders[0]["orderBy"];
            var OrderProperty = EntityType.GetProperty(OrderBy,
                                                       BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance);
            var PropertyAccess    = Expression.MakeMemberAccess(EntityParameter, OrderProperty);
            var OrderByExpression = Expression.Lambda(PropertyAccess, EntityParameter);

            var ResultExpression = Expression.Call(typeof(Queryable),
                                                   MethodName,
                                                   new[] {EntityType, OrderProperty.PropertyType},
                                                   source.Expression,
                                                   Expression.Quote(OrderByExpression));

            var Orders = orders.TakeLast(orders.Count - 1);

            foreach (var Order in Orders)
            {
                OrderBy   = Order["orderBy"];
                OrderType = Order["orderType"];

                MethodName = OrderType == "asc"
                    ? "ThenBy"
                    : "ThenByDescending";

                OrderProperty = EntityType.GetProperty(OrderBy,
                                                       BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance);
                PropertyAccess    = Expression.MakeMemberAccess(EntityParameter, OrderProperty);
                OrderByExpression = Expression.Lambda(PropertyAccess, EntityParameter);

                ResultExpression = Expression.Call(typeof(Queryable),
                                                   MethodName,
                                                   new[] {EntityType, OrderProperty.PropertyType},
                                                   ResultExpression,
                                                   Expression.Quote(OrderByExpression));
            }


            return source.Provider.CreateQuery<T>(ResultExpression)
                         .AsQueryable();
        }

        public static IQueryable<T> SortByDescending<T>(this IQueryable<T> source, string columnName)
        {
            if (string.IsNullOrEmpty(columnName)) return source;

            var Parameter = Expression.Parameter(source.ElementType, "");

            var Property = Expression.Property(Parameter, columnName);
            var Lambda   = Expression.Lambda(Property, Parameter);

            var MethodName = "OrderByDescending";

            Expression MethodCallExpression = Expression.Call(typeof(Queryable), MethodName,
                                                              new[] {source.ElementType, Property.Type},
                                                              source.Expression, Expression.Quote(Lambda));

            return source.Provider.CreateQuery<T>(MethodCallExpression)
                         .AsQueryable();
        }

        // public void TranslateInto(string[] companies) 
        // { 
        //     IQueryable<String>  queryableData = companies.AsQueryable(); 
        //     ParameterExpression pe            = Expression.Parameter(typeof (string), "company"); 
        //     Expression          right         = Expression.Constant("coho winery"); 
        //     Expression          equal         = Expression.Equal(pe, right); 
        //     MethodCallExpression whereCallExpression = Expression.Call( 
        //                                                                typeof (Queryable), 
        //                                                                "Where", 
        //                                                                new[] {queryableData.ElementType}, 
        //                                                                queryableData.Expression, 
        //                                                                Expression.Lambda<Func<string, bool>>(equal, new[] {pe})); 
        //     // ***** End Where ***** 
        //     IQueryable<string> resulList = queryableData.Provider.CreateQuery<string>(whereCallExpression); 
        //     resulList.Dump();
        //     Console.WriteLine ("........................ ");
        //     foreach (string company in resulList)
        //     {
        //         Console.WriteLine (company);
        //     }
        //     Console.WriteLine (",,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,");
        //     return; 
        // }
    }
}