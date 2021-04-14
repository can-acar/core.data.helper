using System;
using System.Linq.Expressions;

namespace Core.Data.Helper.Extensions
{
#pragma warning disable CS8603
    public static class ObjectExtensions
    {
        public static T GetPropertyValue<T>(this object obj, string property)
        {
            return (T) obj.GetType()
                .GetProperty(property)
                ?.GetValue(obj, null);
        }

        public static T GetPropertyValue<T>(this object obj, Expression<Func<object, object>> property)
        {
            var propertyName = ExpressionHelper.GetPropertyName(property);
            return (T) obj.GetType()
                .GetProperty(propertyName)
                ?.GetValue(obj, null);
        }
#pragma warning disable CS8603

        public static string GetPropertyName(this object obj, Expression<Func<object, object>> property)
        {
            var propertyName = ExpressionHelper.GetPropertyName(property);
            return obj.GetType()
                .GetProperty(propertyName)
                ?.Name;
        }
    }

    internal static class ExpressionHelper
    {
        /// <summary>
        /// static string GetPropertyName(Expression<Func<object, object>> property)
        /// </summary>
        /// <param name="property"></param>
        /// <returns>string</returns>
#pragma warning disable SA1402
        public static string GetPropertyName(Expression<Func<object, object>> property)
        {
            var expr = property.Body;
            var propertyName = string.Empty;

            if (expr is UnaryExpression)
                propertyName =
                    ((MemberExpression)
                        ((UnaryExpression) property.Body).Operand).Member.Name;
            else if (expr is MemberExpression) propertyName = ((MemberExpression) property.Body).Member.Name;

            return propertyName;
        }
    }
}