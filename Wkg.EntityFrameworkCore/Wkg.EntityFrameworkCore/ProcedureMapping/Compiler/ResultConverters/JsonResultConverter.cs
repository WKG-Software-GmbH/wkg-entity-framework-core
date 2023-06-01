using System.Linq.Expressions;
using System.Reflection;
using System.Text.Json;
using Wkg.Reflection;

namespace Wkg.EntityFrameworkCore.ProcedureMapping.Compiler.ResultConverters;

/// <summary>
/// Represents a converter that can be used to convert a JSON string to a CLR object.
/// </summary>
public static class JsonResultConverter
{
    private static readonly MethodInfo Deserialize = typeof(JsonSerializer)
        .GetMethod(nameof(JsonSerializer.Deserialize), TypeArray.Of<string, Type, JsonSerializerOptions>())!;

    /// <summary>
    /// Returns a <see cref="LambdaExpression"/> that can be used to convert a JSON string to a CLR object of the specified type.
    /// </summary>
    /// <param name="targetType">The type of the object to convert to.</param>
    public static LambdaExpression For(Type targetType)
    {
        ParameterExpression jsonExpression = Expression.Parameter(typeof(string), "json");
        ConstantExpression typeExpression = Expression.Constant(targetType, typeof(Type));
        ConstantExpression optionsExpression = Expression.Constant(null, typeof(JsonSerializerOptions));
        MethodCallExpression deserializeInvocation = Expression.Call(Deserialize, jsonExpression, typeExpression, optionsExpression);
        Expression result;
        if (targetType.IsValueType)
        {
            result = Expression.Convert(deserializeInvocation, targetType);
        }
        else
        {
            MethodInfo unsafeAs = UnsafeReflection.As(targetType);
            result = Expression.Call(unsafeAs, deserializeInvocation);
        }
        return Expression.Lambda(result, jsonExpression);
    }

    /// <summary>
    /// Returns a <see cref="LambdaExpression"/> that can be used to convert a JSON string to a CLR object of the specified type.
    /// </summary>
    /// <typeparam name="TResult">The type of the object to convert to.</typeparam>
    public static LambdaExpression For<TResult>() => For(typeof(TResult));
}
