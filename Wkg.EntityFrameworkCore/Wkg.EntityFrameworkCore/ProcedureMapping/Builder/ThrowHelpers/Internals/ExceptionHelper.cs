using System.Runtime.CompilerServices;

namespace Wkg.EntityFrameworkCore.ProcedureMapping.Builder.ThrowHelpers.Internals;

internal static class ExceptionHelper
{
    [UnsafeAccessor(UnsafeAccessorKind.Field, Name = "_message")]
    private static extern ref string GetExceptionMessageByRef(Exception exception);

    [UnsafeAccessor(UnsafeAccessorKind.Field, Name = "_paramName")]
    private static extern ref string? GetArgumentNameByRef(ArgumentException exception);

    public static void SetExceptionMessage(Exception target, string message) => GetExceptionMessageByRef(target) = message;

    public static void SetArgumentName(ArgumentException target, string? parameterName) => GetArgumentNameByRef(target) = parameterName;
}