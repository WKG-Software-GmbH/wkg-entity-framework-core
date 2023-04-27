using System.Diagnostics.CodeAnalysis;
using System.Diagnostics;
using System.Reflection;

namespace Wkg.EntityFrameworkCore.ProcedureMapping.Builder.ThrowHelpers;

internal abstract class ThrowHelperBase : IThrowHelper
{
    protected abstract string TargetSite { get; }

    [DoesNotReturn]
    [StackTraceHidden]
    public void Throw<TException>(string message) where TException : Exception, new()
    {
        ExceptionProxy<TException> proxy = new($"'{message}' ({TargetSite})");
        proxy.Throw();
    }

    [DoesNotReturn]
    [StackTraceHidden]
    public T Throw<TException, T>(string message) where TException : Exception, new()
    {
        Throw<TException>(message);
        return default;
    }

    [DoesNotReturn]
    [StackTraceHidden]
    public void Throw<TArgumentException>(string message, string paramName) where TArgumentException : ArgumentException, new()
    {
        ArgumentExceptionProxy<TArgumentException> proxy = new($"'{message}' ({TargetSite})", paramName);
        proxy.Throw();
    }

    [DoesNotReturn]
    [StackTraceHidden]
    public T Throw<TArgumentException, T>(string message, string paramName) where TArgumentException : ArgumentException, new()
    {
        Throw<TArgumentException>(message, paramName);
        return default;
    }
}

file readonly struct ExceptionProxy<TException> where TException : Exception, new()
{
    private readonly Exception _proxiedException;

    public ExceptionProxy(string message)
    {
        TException proxiedException = new();
        if (ReflectionHelper.TrySetExceptionMessage(proxiedException, message))
        {
            _proxiedException = proxiedException;
        }
        else
        {
            _proxiedException = new ProcedureBuilderException(message);
        }
    }
    
    [DoesNotReturn]
    [StackTraceHidden]
    public void Throw() => throw _proxiedException;
}

file readonly struct ArgumentExceptionProxy<TArgumentException> where TArgumentException : ArgumentException, new()
{
    private readonly ArgumentException _proxiedException;

    public ArgumentExceptionProxy(string message, string? parameterName)
    {
        TArgumentException proxiedException = new();
        if (ReflectionHelper.TrySetExceptionMessage(proxiedException, message) && ReflectionHelper.TrySetArgumentName(proxiedException, parameterName))
        {
            _proxiedException = proxiedException;
        }
        else
        {
            _proxiedException = new ArgumentException(message, parameterName);
        }
    }

    [DoesNotReturn]
    [StackTraceHidden]
    public void Throw() => throw _proxiedException;
}

file static class ReflectionHelper
{
    public static bool TrySetExceptionMessage<TException>(TException target, string message) => 
        TrySetPrivateField(target, "_message", message);

    public static bool TrySetArgumentName<TArgumentException>(TArgumentException target, string? parameterName) => 
        TrySetPrivateField(target, "_paramName", parameterName);

    private static bool TrySetPrivateField<TTarget, TValue>(TTarget target, string fieldName, TValue value)
    {
        FieldInfo? field = typeof(TTarget).GetField(fieldName, BindingFlags.Instance | BindingFlags.NonPublic);
        if (field is null)
        {
            return false;
        }
        field.SetValue(target, value);
        return true;
    }
}