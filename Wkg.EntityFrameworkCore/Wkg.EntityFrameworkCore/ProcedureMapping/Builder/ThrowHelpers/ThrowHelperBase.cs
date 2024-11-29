using System.Diagnostics.CodeAnalysis;
using System.Diagnostics;
using Wkg.EntityFrameworkCore.ProcedureMapping.Builder.ThrowHelpers.Internals;
using Wkg.Logging;
using Wkg.Logging.Writers;

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

    [StackTraceHidden]
    public void Warn(string message) => Log.WriteWarning($"'{message}' ({TargetSite})", LogWriter.Blocking);

    [StackTraceHidden]
    public void Warn(string message, string paramName) => Log.WriteWarning($"'{message}' ({TargetSite})", LogWriter.Blocking);
}
