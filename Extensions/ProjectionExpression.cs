using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text.RegularExpressions;

namespace Core.Data.Helper.Extensions
{
    public class ProjectionExpression<TSource>
    {
        // ReSharper disable once StaticMemberInGenericType
        private static readonly Dictionary<string, Expression> ExpressionCache = new();

        private readonly IQueryable<TSource> Source;

        public ProjectionExpression(IQueryable<TSource> source)
        {
            Source = source;
        }

        public IQueryable<TDest> To<TDest>()
        {
            var queryExpression = GetCachedExpression<TDest>() ?? BuildExpression<TDest>();

            return Source.Select(queryExpression);
        }
#pragma warning disable CS8603
        private static Expression<Func<TSource, TDest>> GetCachedExpression<TDest>()
        {
            var key = GetCacheKey<TDest>();

            return ExpressionCache.ContainsKey(key)
                ? ExpressionCache[key] as Expression<Func<TSource, TDest>>
                : null;
        }

        private static Expression<Func<TSource, TDest>> BuildExpression<TDest>()
        {
            var sourceProperties = typeof(TSource).GetProperties();
            var destinationProperties = typeof(TDest).GetProperties()
                                                     .Where(dest => dest.CanWrite);
            var parameterExpression = Expression.Parameter(typeof(TSource), "src");

            var propertyInfos = destinationProperties.ToList();
            var bindings = propertyInfos.Select(destinationProperty => BuildBinding(parameterExpression, destinationProperty, sourceProperties))
                                        .Where(binding => binding != null);

            Expression.Lambda<Func<TSource, TDest>>(Expression.MemberInit(Expression.New(typeof(TDest)), bindings), parameterExpression);

            bindings = propertyInfos
                       .Select(destinationProperty => BuildBinding(parameterExpression, destinationProperty, sourceProperties))
                       .Where(binding => binding != null);

            var expressionLambda = Expression.Lambda<Func<TSource, TDest>>(Expression.MemberInit(Expression.New(typeof(TDest)), bindings),
                                                                           parameterExpression);

            var key = GetCacheKey<TDest>();

            ExpressionCache.Add(key, expressionLambda);

            return expressionLambda;
        }
#pragma warning restore CS8603
        private static MemberAssignment BuildBinding(Expression parameterExpression, MemberInfo destinationProperty, IEnumerable<PropertyInfo> sourceProperties)
        {
            IEnumerable<PropertyInfo> propertyInfos = sourceProperties as PropertyInfo[] ?? sourceProperties.ToArray();

            var sourceProperty = propertyInfos.FirstOrDefault(src => src.Name == destinationProperty.Name);

            if (sourceProperty != null) return Expression.Bind(destinationProperty, Expression.Property(parameterExpression, sourceProperty));

            var propertyNames = SplitCamelCase(destinationProperty.Name);
#pragma warning disable CS8603
            if (propertyNames.Length != 2) return null;

            propertyNames = SplitCamelCase(destinationProperty.Name);
#pragma warning disable CS8603
            if (propertyNames.Length != 2) return null;
            {
                sourceProperty = propertyInfos.FirstOrDefault(src => src.Name == propertyNames[0]);
#pragma warning disable CS8603
                if (sourceProperty == null) return null;
                {
                    var sourceChildProperty = sourceProperty.PropertyType.GetProperties()
                                                            .FirstOrDefault(src => src.Name == propertyNames[1]);

                    return Expression.Bind(destinationProperty,
                                           Expression
                                               .Property(Expression.Property(parameterExpression, sourceProperty),
                                                         sourceChildProperty ?? throw new ArgumentNullException()));
                }
            }
#pragma warning disable CS8603
        }

        private static string GetCacheKey<TDest>()
        {
            return string.Concat(typeof(TSource).FullName, typeof(TDest).FullName);
        }

        private static string[] SplitCamelCase(string input)
        {
            return Regex.Replace(input, "([A-Z])", " $1", RegexOptions.Compiled)
                        .Trim()
                        .Split(' ');
        }
    }
}