using Microsoft.EntityFrameworkCore.Infrastructure;

namespace Wkg.EntityFrameworkCore.Extensions;

/// <summary>
/// Provides extension methods for <see cref="IReadOnlyAnnotatable"/>.
/// </summary>
public static class ReadOnlyAnnotatableExtensions
{
    /// <summary>
    /// Checks whether the <see cref="IReadOnlyAnnotatable"/> has an annotation with the specified name.
    /// </summary>
    /// <param name="annotatable">The <see cref="IReadOnlyAnnotatable"/> to check.</param>
    /// <param name="name">The name of the annotation.</param>
    /// <returns><see langword="true"/> if the <see cref="IReadOnlyAnnotatable"/> has an annotation with the specified name; otherwise, <see langword="false"/>.</returns>
    public static bool HasAnnotation(this IReadOnlyAnnotatable annotatable, string name) => 
        annotatable.FindAnnotation(name) is not null;

    /// <summary>
    /// Checks whether the <see cref="IReadOnlyAnnotatable"/> has an annotation matching the specified predicate.
    /// </summary>
    /// <param name="annotatable">The <see cref="IReadOnlyAnnotatable"/> to check.</param>
    /// <param name="predicate">The predicate to match.</param>
    /// <returns><see langword="true"/> if the <see cref="IReadOnlyAnnotatable"/> has an annotation matching the specified predicate; otherwise, <see langword="false"/>.</returns>
    public static bool HasAnnotation(this IReadOnlyAnnotatable annotatable, Func<string, bool> predicate) => 
        annotatable.GetAnnotations().Any(a => predicate(a.Name));
}
