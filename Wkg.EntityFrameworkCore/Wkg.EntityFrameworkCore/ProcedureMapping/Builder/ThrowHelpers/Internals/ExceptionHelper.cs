using System.Runtime.CompilerServices;

namespace Wkg.EntityFrameworkCore.ProcedureMapping.Builder.ThrowHelpers.Internals;

internal static class ExceptionHelper
{
    [UnsafeAccessor(UnsafeAccessorKind.Field, Name = "_message")]
    private static extern ref string GetSetExceptionMessage(Exception exception);

    [UnsafeAccessor(UnsafeAccessorKind.Field, Name = "_paramName")]
    private static extern ref string? GetSetArgumentName(ArgumentException exception);

    public static void SetExceptionMessage(Exception target, string message) => GetSetExceptionMessage(target) = message;

    public static void SetArgumentName(ArgumentException target, string? parameterName) => GetSetArgumentName(target) = parameterName;
}