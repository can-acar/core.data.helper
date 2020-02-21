using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text.RegularExpressions;

namespace core.data.helper.extensions
{
    public class ProjectionExpression<TSource>
    {
        // ReSharper disable once StaticMemberInGenericType
        private static readonly Dictionary<string, Expression> ExpressionCache = new Dictionary<string, Expression>();

        private readonly IQueryable<TSource> Source;

        public ProjectionExpression(IQueryable<TSource> source)
        {
            Source = source;
        }

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
            var SourceProperties = typeof(TSource).GetProperties();
            var DestinationProperties = typeof(TDest).GetProperties().Where(dest => dest.CanWrite);
            var ParameterExpression = Expression.Parameter(typeof(TSource), "src");

            var PropertyInfos = DestinationProperties.ToList();
            var Bindings = PropertyInfos
                .Select(destinationProperty =>
                    BuildBinding(ParameterExpression, destinationProperty, SourceProperties))
                .Where(binding => binding != null);

            Expression.Lambda<Func<TSource, TDest>>(Expression.MemberInit(Expression.New(typeof(TDest)), Bindings),
                ParameterExpression);

            Bindings = PropertyInfos
                .Select(destinationProperty =>
                    BuildBinding(ParameterExpression, destinationProperty, SourceProperties))
                .Where(binding => binding != null);

            var ExpressionLambda = Expression.Lambda<Func<TSource, TDest>>(
                Expression.MemberInit(Expression.New(typeof(TDest)), Bindings),
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

                    return Expression.Bind(destinationProperty,
                        Expression
                            .Property(Expression.Property(parameterExpression, SourceProperty),
                                SourceChildProperty ?? throw new ArgumentNullException()));
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
            return Regex.Replace(input, "([A-Z])", " $1", RegexOptions.Compiled).Trim().Split(' ');
        }
    }
}