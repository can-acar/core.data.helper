using System.Linq.Expressions;
using System.Reflection;
using CoreEntityHelper.Infrastructures;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;

namespace CoreEntityHelper.Extensions;

public static class QueryableExtensions
{
    public static ProjectionExpression<TSource> Project<TSource>(this IQueryable<TSource> source)
    {
        return new ProjectionExpression<TSource>(source);
    }

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

    public static IQueryable<TResult> InnerJoin<TLeftSource, TRightSource, TQueryKey, TResult>(
        this IRepository<TLeftSource> source,
        IRepository<TRightSource> inner,
        Expression<Func<TLeftSource, TQueryKey>> left,
        Expression<Func<TRightSource, TQueryKey>> right,
        Expression<Func<TLeftSource, TRightSource, TResult>> result)
        where TLeftSource : class where TRightSource : class
    {
        return source.AsQueryable()
            .Join(inner.AsQueryable(), left, right, result);
    }


    public static IQueryable<TResult> LeftJoin<TLeftSource, TRightSource, TQueryKey, TResult>(
        this IRepository<TLeftSource> source,
        IRepository<TRightSource> inner,
        Expression<Func<TLeftSource, TQueryKey>> left,
        Expression<Func<TRightSource, TQueryKey>> right,
        Expression<Func<TLeftSource, TRightSource, TResult>> result)
        where TLeftSource : class where TRightSource : class
    {
        return source.AsQueryable()
            .GroupJoin(
                inner.AsQueryable(),
                left,
                right,
                (o, i) => new {o, i}
            )
            .SelectMany(
                x => x.i.DefaultIfEmpty(),
                (x, i) => new {x.o, i}
            )
            .Select(x => result.Compile().Invoke(x.o, x.i));
    }

    public static IQueryable<TResult> LeftJoin<TOuter, TInner, TKey, TResult>(
        this IQueryable<TOuter> outer,
        IRepository<TInner> inner,
        Expression<Func<TOuter, TKey>> outerKeySelector,
        Expression<Func<TInner, TKey>> innerKeySelector,
        Expression<Func<TOuter, TInner, TResult>> resultSelector) where TInner : class where TOuter : class
    {
        return outer
            .GroupJoin(
                inner.AsQueryable(),
                outerKeySelector,
                innerKeySelector,
                (o, i) => new {o, i}
            )
            .SelectMany(
                x => x.i.DefaultIfEmpty(),
                (x, i) => new {x.o, i}
            )
            .Select(x => resultSelector.Compile().Invoke(x.o, x.i));
    }


    public static IQueryable<TResult> LeftJoin<TOuter, TInner, TKey, TResult>(
        this IQueryable<TOuter> outer,
        IEnumerable<TInner> inner,
        Expression<Func<TOuter, TKey>> outerKeySelector,
        Expression<Func<TInner, TKey>> innerKeySelector,
        Expression<Func<TOuter, TInner, TResult>> resultSelector)
    {
        return outer
            .GroupJoin(
                inner,
                outerKeySelector,
                innerKeySelector,
                (o, i) => new {o, i}
            )
            .SelectMany(
                x => x.i.DefaultIfEmpty(),
                (x, i) => new {x.o, i}
            )
            .Select(x => resultSelector.Compile().Invoke(x.o, x.i));
    }


    public static IQueryable<TResult> LeftOuterJoin<TSource, TInner, TKey, TResult>(this IRepository<TSource> source,
        IRepository<TInner> inner,
        Func<TSource, TKey> left,
        Func<TInner, TKey> right,
        Func<TSource, TInner, TResult> res) where TSource : class where TInner : class
    {
        return from f in source.AsQueryable()
            join b in inner.AsQueryable() on left.Invoke(f) equals right.Invoke(b) into g
            from result in g.DefaultIfEmpty()
            select res.Invoke(f, result);
    }

    public static IQueryable<TEntity> Pagination<TEntity>(this IRepository<TEntity> source,
        int currentPage,
        int limit,
        out int rowCount) where TEntity : class
    {
        rowCount = source.Count();

        return source.Entity
            .Skip((currentPage - 1) * limit)
            .Take(limit)
            .AsQueryable();
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

    public static async Task<HashSet<TEntity>> ToHashSetAsync<TEntity>(this IQueryable<TEntity> source,
        CancellationToken cancellationToken = default)
    {
        var asyncEnumerator = source.AsAsyncEnumerable().GetAsyncEnumerator(cancellationToken);
        var result = new HashSet<TEntity>();

        try
        {
            while (true)
            {
                var hasNext = await asyncEnumerator.MoveNextAsync();

                if (!hasNext) break;

                result.Add(asyncEnumerator.Current);
            }
        }
        finally
        {
            await asyncEnumerator.DisposeAsync();
        }

        asyncEnumerator = null;

        return result;
    }


    public static async Task<HashSet<TEntity>> ToHashSetPaginationAsync<TEntity>(this IQueryable<TEntity> source,
        int currentPage,
        int limit) where TEntity : class
    {
        return await source
            .Skip((currentPage - 1) * limit)
            .Take(limit)
            .ToHashSetAsync();
    }

    public static Task<IQueryable<TSource>> WhereAsync<TSource>(this IQueryable<TSource> source,
        Expression<Func<TSource, bool>> predicate) where TSource : class
    {
        return Task.Run(() => source.Where(predicate));
    }

    public static IQueryable<T> Select<T>(this IQueryable<T> source, string columnName)
    {
        if (string.IsNullOrEmpty(columnName)) return source;

        var parameter = Expression.Parameter(source.ElementType, "p");
        var property = Expression.Property(parameter, columnName);
        var lambda = Expression.Lambda(property, parameter);

        //string methodName = isAscending ? "OrderBy" : "OrderByDescending";  
        const string METHOD_NAME = "Select"; // : "OrderByDescending";  

        Expression methodCallExpression = Expression.Call(typeof(Queryable),
            METHOD_NAME,
            new[] {source.ElementType, property.Type},
            source.Expression,
            Expression.Quote(lambda));

        return source.Provider.CreateQuery<T>(methodCallExpression)
            .AsQueryable();
    }

    private static class PropertyAccessorCache<T> where T : class
    {
        private static IDictionary<string, LambdaExpression> Cache { get; }

        static PropertyAccessorCache()
        {
            var storage = new Dictionary<string, LambdaExpression>();

            var T = typeof(T);
            var parameter = Expression.Parameter(T, "p");

            foreach (var property in T.GetProperties(BindingFlags.Public | BindingFlags.Instance))
            {
                var propertyAccess = Expression.MakeMemberAccess(parameter, property);
                var lambdaExpression = Expression.Lambda(propertyAccess, parameter);
                storage[property.Name] = lambdaExpression;
            }

            Cache = storage;
        }

        public static LambdaExpression Get(string propertyName)
        {
            return Cache.TryGetValue(propertyName, out var result)
                ? result
                : null;
        }
    }

    public static IQueryable<T> Filter<T>(this IQueryable<T> source, string propertyName, object propertyValue)
        where T : class
    {
        var param = Expression.Parameter(typeof(T), typeof(T).Name);
        var property = Expression.Property(param, propertyName.FirstUpper());
        var searchValue = Convert.ChangeType(propertyValue, property.Type);

        Expression matchExpression = property;
        MethodCallExpression methodCallExpression = null;

        if (matchExpression.Type == typeof(bool))
        {
            // propertyValue  change to bit
            searchValue = (string) propertyValue == "true" ? true : false; //Convert.ChangeType(1, typeof(byte)) : Convert.ChangeType(0, typeof(byte));

            matchExpression = Expression.Equal(property, Expression.Constant(searchValue));

            methodCallExpression = Expression.Call(
                typeof(Queryable),
                "Where",
                new Type[] {source.ElementType},
                source.Expression,
                Expression.Lambda(matchExpression, param)
            );

            return source.Where(Expression.Lambda<Func<T, bool>>(matchExpression, param)).AsQueryable();

            //var result = source.Provider.CreateQuery<T>(whereCall);


            // return source.Where(lambda).AsQueryable();
        }
        else
        {
            var pattern = Expression.Constant($"%{searchValue}%");
            matchExpression = Expression.Convert(matchExpression, typeof(object));
            matchExpression = Expression.Convert(matchExpression, typeof(string));
            var likeMethod = typeof(DbFunctionsExtensions).GetMethod("Like", new[] {typeof(DbFunctions), property.Type, property.Type});

            methodCallExpression = Expression.Call(null,
                likeMethod,
                Expression.Constant(EF.Functions),
                matchExpression,
                pattern);
        }


        return source.Where(Expression.Lambda<Func<T, bool>>(methodCallExpression, param)).AsQueryable();
    }

    // public static IQueryable<TResult> SelectRecursive<TSource, TResult>(
    //     this IQueryable<TSource> source,
    //     Expression<Func<TSource, TResult>> template,
    //     Expression<Func<TResult, IEnumerable<TResult>>> recursivePath)
    // {
    //     var result = source.Select(template);
    //
    //     var paramTemplate = template.Parameters[0];
    //     var paramRecursive = recursivePath.Parameters[0];
    //
    //     var paramRecursiveLambda = Expression.Lambda<Func<TSource, IEnumerable<TResult>>>(Expression.Invoke(recursivePath, paramTemplate), paramTemplate);
    //
    //     var recursiveResult = result
    //         .SelectMany<TSource,TResult>(paramRecursiveLambda.Compile())
    //         .SelectRecursive(template, recursivePath);
    //
    //     //.SelectMany(paramRecursiveLambda.Compile()).SelectRecursive(template, recursivePath);
    //
    //     return result.Concat(recursiveResult);
    // }

    // public static IQueryable<TResult> SelectRecursive<TSource, TResult>(this IQueryable<TSource> source, Expression<Func<TSource, TResult>> template, Expression<Func<TResult,
    //     IEnumerable<TResult>>> recursivePath)
    // {
    //     var result = source.Select(template);
    //     var resultExpression = (MemberInitExpression) template.Body;
    //     var recursiveProperty = (MemberExpression) recursivePath.Body;
    //     var recursiveBinding = resultExpression.Bindings
    //         .OfType<MemberAssignment>()
    //         .Single(b => b.Member == recursiveProperty.Member);
    //     var newBindings = resultExpression.Bindings
    //         .Where(b => b != recursiveBinding)
    //         .Append(Expression.Bind(
    //             recursiveBinding.Member,
    //             Expression.Call(
    //                 typeof(Enumerable),
    //                 nameof(Enumerable.SelectMany),
    //                 new[] {typeof(TResult), typeof(TResult)}, // Changed from {typeof(TSource), typeof(TResult)}
    //                 recursiveBinding.Expression, // Changed from recursiveBinding.Expression
    //                 Expression.Lambda(
    //                     Expression.Invoke(
    //                         template,
    //                         Expression.Parameter(typeof(TSource), "x")),
    //                     Expression.Parameter(typeof(TSource), "x")))));
    //     var newTemplate = Expression.Lambda<Func<TSource, TResult>>(Expression.MemberInit(resultExpression.NewExpression, newBindings), template.Parameters);
    //
    //     return result.Concat(result.AsEnumerable().SelectMany(recursivePath.Compile()).SelectMany(x =>
    //     {
    //         var recursiveResult = SelectRecursive(new[] {x}, template, recursivePath).AsQueryable();
    //         return recursiveResult;
    //     }));
    //
    //     // return result.Concat(result.SelectMany(recursivePath).SelectMany(x =>
    //     // {
    //     //     var recursiveResult = SelectRecursive(x, newTemplate, recursivePath);
    //     //     return recursiveResult;
    //     // }));
    // }


    // public static IQueryable<TResult> SelectRecursive<TSource, TResult>(this IQueryable<TSource> source,
    //     Expression<Func<TSource, TResult>> template,
    //     Expression<Func<TResult, IEnumerable<TResult>>> recursivePath)
    // {
    //     var result = source.Select(template);
    //     var resultExpression = (MemberInitExpression) template.Body;
    //     var recursiveProperty = (MemberExpression) recursivePath.Body;
    //     var recursiveBinding = resultExpression.Bindings
    //         .OfType<MemberAssignment>()
    //         .Single(b => b.Member == recursiveProperty.Member);
    //     var newBindings = resultExpression.Bindings
    //         .Where(b => b != recursiveBinding)
    //         .Append(Expression.Bind(
    //             recursiveBinding.Member,
    //             Expression.Call(
    //                 typeof(Enumerable),
    //                 nameof(Enumerable.SelectMany),
    //                 new[] {typeof(TSource), typeof(TResult)},
    //                 Expression.Property(recursiveBinding.Expression, nameof(IEnumerable<TSource>.AsQueryable)),
    //                 Expression.Lambda(
    //                     Expression.Invoke(
    //                         template,
    //                         Expression.Parameter(typeof(TSource), "x")),
    //                     Expression.Parameter(typeof(TSource), "x")))));
    //     var newTemplate = Expression.Lambda<Func<TSource, TResult>>(
    //         Expression.MemberInit(resultExpression.NewExpression, newBindings),
    //         template.Parameters);
    //     return result.Concat(result.SelectMany(recursivePath).SelectMany(x => x.AsQueryable().SelectRecursive(newTemplate, recursivePath)));
    // }

    public static IQueryable<T> Where<T>(this IQueryable<T> source, string propertyName, object propertyValue, out bool success)
        where T : class
    {
        success = false;
        var mba = PropertyAccessorCache<T>.Get(propertyName);

        if (mba == null) return source;

        object value;

        try
        {
            value = Convert.ChangeType(propertyValue, mba.ReturnType);
        }
        catch (SystemException ex) when (ex is InvalidCastException ||
                                         ex is FormatException ||
                                         ex is OverflowException ||
                                         ex is ArgumentNullException)
        {
            return source;
        }

        var eqe = Expression.Equal(mba.Body, Expression.Constant(value, mba.ReturnType));

        var queryExpr = Expression.Lambda(eqe, mba.Parameters[0]);

        success = true;

        var resultExpression = Expression.Call(null,
            GetMethodInfo<IQueryable<T>,
                Expression<Func<T, bool>>,
                IQueryable<T>>(Queryable.Where),
            new[] {source.Expression, Expression.Quote(queryExpr)});

        return source.Provider.CreateQuery<T>(resultExpression);
    }

    private static MethodInfo GetMethodInfo<T1, T2, T3>(Func<T1, T2, T3> f)
    {
        return f.Method;
    }

    public static IQueryable<T> SortBy<T>(this IQueryable<T> source, string columnName)
    {
        if (string.IsNullOrEmpty(columnName)) return source;

        var parameter = Expression.Parameter(source.ElementType, "");
        var property = Expression.Property(parameter, columnName);
        var lambda = Expression.Lambda(property, parameter);

        //string methodName = isAscending ? "OrderBy" : "OrderByDescending";  
        var methodName = "OrderBy"; // : "OrderByDescending";  

        Expression methodCallExpression = Expression.Call(typeof(Queryable),
            methodName,
            new[] {source.ElementType, property.Type},
            source.Expression,
            Expression.Quote(lambda));

        return source.Provider.CreateQuery<T>(methodCallExpression)
            .AsQueryable();
    }

    public static IQueryable<T> SortBy<T>(this IQueryable<T> source, IEnumerable<string> columnNames)
    {
        if (source == null) throw new ArgumentNullException(nameof(source));

        var queryExpr = source.Expression;
        var parameter = Expression.Parameter(source.ElementType, "");
        var methodName = "OrderBy";

        foreach (var columnName in columnNames)
        {
            var property = Expression.Property(parameter, columnName);
            var lambda = Expression.Lambda(property, parameter);

            queryExpr = Expression.Call(typeof(Queryable),
                methodName,
                new[] {source.ElementType, property.Type},
                queryExpr,
                Expression.Quote(lambda));

            methodName = "ThenBy";
        }

        return source.Provider.CreateQuery<T>(queryExpr)
            .AsQueryable();
    }

    public static IQueryable<T> SortBy<T>(this IQueryable<T> source, IList<IDictionary<string, string>> orders)
    {
        if (source == null) throw new ArgumentNullException(nameof(source));

        var entityType = source.ElementType;

        var entityParameter = Expression.Parameter(entityType, "p");

        var orderType = orders[0]["orderType"];

        var methodName = orderType == "asc"
            ? "OrderBy"
            : "OrderByDescending";

        var orderBy = orders[0]["orderBy"].FirstUpper();
        var orderProperty = entityType.GetProperty(orderBy, BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance);
        var propertyAccess = Expression.MakeMemberAccess(entityParameter, orderProperty!);
        var orderByExpression = Expression.Lambda(propertyAccess, entityParameter);
        var resultExpression = Expression.Call(typeof(Queryable), methodName, new[] {entityType, orderProperty.PropertyType}, source.Expression,
            Expression.Quote(orderByExpression));

        var items = orders.TakeLast(orders.Count - 1);

        foreach (var order in items)
        {
            orderBy = order["orderBy"].FirstUpper();
            orderType = order["orderType"];

            methodName = orderType == "asc"
                ? "ThenBy"
                : "ThenByDescending";

            orderProperty = entityType.GetProperty(orderBy,
                BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance);

            propertyAccess = Expression.MakeMemberAccess(entityParameter, orderProperty!);

            orderByExpression = Expression.Lambda(propertyAccess, entityParameter);

            resultExpression = Expression.Call(typeof(Queryable),
                methodName,
                new[] {entityType, orderProperty.PropertyType},
                resultExpression,
                Expression.Quote(orderByExpression));
        }

        return source.Provider.CreateQuery<T>(resultExpression)
            .AsQueryable();
    }

    public static IQueryable<T> SortByDescending<T>(this IQueryable<T> source, string columnName)
    {
        if (string.IsNullOrEmpty(columnName)) return source;

        var parameter = Expression.Parameter(source.ElementType, "");
        var property = Expression.Property(parameter, columnName);
        var lambda = Expression.Lambda(property, parameter);
        var methodName = "OrderByDescending";

        Expression methodCallExpression = Expression.Call(typeof(Queryable),
            methodName,
            new[] {source.ElementType, property.Type},
            source.Expression,
            Expression.Quote(lambda));

        return source.Provider.CreateQuery<T>(methodCallExpression)
            .AsQueryable();
    }

    /// <summary>
    /// SplitQuery() is not a built-in LINQ method.
    /// However, it's possible you're referring to a custom extension method that's used to split large queries into smaller chunks, which can help improve performance and avoid running into SQL query limitations.
    /// 
    public static IEnumerable<List<T>> SplitQuery<T>(this IQueryable<T> query, int chunkSize)
    {
        if (chunkSize <= 0)
        {
            throw new ArgumentException("Chunk size must be greater than 0.", nameof(chunkSize));
        }

        var skip = 0;
        List<T> chunk;
        do
        {
            chunk = query.Skip(skip).Take(chunkSize).ToList();
            skip += chunkSize;

            if (chunk.Count > 0)
            {
                yield return chunk;
            }
        } while (chunk.Count == chunkSize);
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