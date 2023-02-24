using System.Linq.Expressions;

namespace CoreEntityHelper.Extensions;
#pragma warning disable CS8603
#pragma warning disable SA1402
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