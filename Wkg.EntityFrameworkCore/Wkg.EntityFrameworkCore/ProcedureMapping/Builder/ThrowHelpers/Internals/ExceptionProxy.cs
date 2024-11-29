using System.Diagnostics.CodeAnalysis;
using System.Diagnostics;

namespace Wkg.EntityFrameworkCore.ProcedureMapping.Builder.ThrowHelpers.Internals;

internal readonly struct ExceptionProxy<TException> where TException : Exception, new()
{
    private readonly Exception _proxiedException;

    public ExceptionProxy(string message)
    {
        TException proxiedException = new();
        ExceptionHelper.SetExceptionMessage(proxiedException, message);
        _proxiedException = proxiedException;
    }

    [DoesNotReturn]
    [StackTraceHidden]
    public void Throw() => throw _proxiedException;
}
