using Microsoft.EntityFrameworkCore.Infrastructure;

namespace Wkg.EntityFrameworkCore.Extensions;

internal static class ReadOnlyAnnotatableExtensions
{
    public static bool HasAnnotation(this IReadOnlyAnnotatable annotatable, string name) => 
        annotatable.FindAnnotation(name) is not null;

    public static bool HasAnnotation(this IReadOnlyAnnotatable annotatable, Func<string, bool> predicate) => 
        annotatable.GetAnnotations().Any(a => predicate(a.Name));
}
