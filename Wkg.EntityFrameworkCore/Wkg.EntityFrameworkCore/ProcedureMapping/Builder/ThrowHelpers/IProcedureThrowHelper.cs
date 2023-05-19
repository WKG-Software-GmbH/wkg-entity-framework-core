using System.Reflection;

namespace Wkg.EntityFrameworkCore.ProcedureMapping.Builder.ThrowHelpers;

/// <summary>
/// Represents a throw helper that can be used to throw exceptions within the context of procedure mapping.
/// </summary>
public interface IProcedureThrowHelper : IThrowHelper
{
    /// <summary>
    /// Creates a new <see cref="IThrowHelper"/> within this procedure mapping context for the paramter represented by the provided <paramref name="property"/>.
    /// </summary>
    /// <param name="property">The property that represents the parameter.</param>
    /// <returns>A new <see cref="IThrowHelper"/> within this procedure mapping context.</returns>
    IThrowHelper ForParameter(PropertyInfo property);

    /// <summary>
    /// Creates a new <see cref="IResultThrowHelper"/> within this procedure mapping context for the result entities returned by the procedure.
    /// </summary>
    /// <typeparam name="TResult">The type of the result entities.</typeparam>
    /// <returns>A new <see cref="IResultThrowHelper"/> within this procedure mapping context.</returns>
    IResultThrowHelper ForResult<TResult>();
}
