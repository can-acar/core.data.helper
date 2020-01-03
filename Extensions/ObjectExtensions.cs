using System;
using System.Linq.Expressions;

namespace core.data.helper.extensions
{
#pragma warning disable CS8603
    public static class ObjectExtensions
    {
        public static T GetPropertyValue<T>(this object obj, string property) { return(T) obj.GetType().GetProperty(property)?.GetValue(obj, null); }

        public static T GetPropertyValue<T>(this object obj, Expression<Func<object, object>> property)
        {
            var PropertyName = ExpressionHelper.GetPropertyName(property);
            return(T) obj.GetType().GetProperty(PropertyName)?.GetValue(obj, null);
        }
        #pragma warning disable CS8603

        public static string GetPropertyName(this object obj, Expression<Func<object, object>> property)
        {
            var PropertyName = ExpressionHelper.GetPropertyName(property);
            return obj.GetType().GetProperty(PropertyName)?.Name;
        }
    }

    internal static class ExpressionHelper
    {
        public static string GetPropertyName(Expression<Func<object, object>> property)
        {
            var Expr = property.Body;
            var PropertyName = string.Empty;

            if(Expr is UnaryExpression)
                PropertyName =
                    ((MemberExpression)
                        ((UnaryExpression) property.Body).Operand).Member.Name;
            else if(Expr is MemberExpression) PropertyName = ((MemberExpression) property.Body).Member.Name;

            return PropertyName;
        }
    }

}