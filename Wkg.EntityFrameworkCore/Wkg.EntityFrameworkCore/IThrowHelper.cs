using System.Diagnostics.CodeAnalysis;
using System.Diagnostics;

namespace Wkg.EntityFrameworkCore.ProcedureMapping.Builder.ThrowHelpers;

public interface IThrowHelper
{
    [DoesNotReturn]
    [StackTraceHidden]
    public void Throw<TException>(string message) where TException : Exception, new();

    [DoesNotReturn]
    [StackTraceHidden]
    public T Throw<TException, T>(string message) where TException : Exception, new();

    [DoesNotReturn]
    [StackTraceHidden]
    public void Throw<TArgumentException>(string message, string paramName) where TArgumentException : ArgumentException, new();

    [DoesNotReturn]
    [StackTraceHidden]
    public T Throw<TArgumentException, T>(string message, string paramName) where TArgumentException : ArgumentException, new();
}