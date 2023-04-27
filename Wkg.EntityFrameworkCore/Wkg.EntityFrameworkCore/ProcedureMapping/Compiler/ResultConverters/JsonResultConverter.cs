using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Wkg.Reflection;

namespace Wkg.EntityFrameworkCore.ProcedureMapping.Compiler.ResultConverters;

public static class JsonResultConverter
{
    private static readonly MethodInfo Deserialize = typeof(JsonSerializer)
        .GetMethod(nameof(JsonSerializer.Deserialize), TypeArray.Of<string, Type, JsonSerializerOptions>())!;

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

    public static LambdaExpression For<TResult>() => For(typeof(TResult));
}
