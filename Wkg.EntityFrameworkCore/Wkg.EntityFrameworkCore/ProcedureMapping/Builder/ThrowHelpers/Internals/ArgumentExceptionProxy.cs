using System.Diagnostics.CodeAnalysis;
using System.Diagnostics;

namespace Wkg.EntityFrameworkCore.ProcedureMapping.Builder.ThrowHelpers.Internals;

internal readonly struct ArgumentExceptionProxy<TArgumentException> where TArgumentException : ArgumentException, new()
{
    private readonly ArgumentException _proxiedException;

    public ArgumentExceptionProxy(string message, string? parameterName)
    {
        TArgumentException proxiedException = new();
        ExceptionHelper.SetExceptionMessage(proxiedException, message);
        if (parameterName is not null)
        {
            ExceptionHelper.SetArgumentName(proxiedException, parameterName);
        }
        _proxiedException = proxiedException;
    }

    [DoesNotReturn]
    [StackTraceHidden]
    public void Throw() => throw _proxiedException;
}
