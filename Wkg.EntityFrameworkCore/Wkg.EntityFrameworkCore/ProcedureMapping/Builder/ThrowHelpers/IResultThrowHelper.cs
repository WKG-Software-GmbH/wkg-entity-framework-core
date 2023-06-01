using System.Reflection;

namespace Wkg.EntityFrameworkCore.ProcedureMapping.Builder.ThrowHelpers;

/// <summary>
/// Represents a throw helper that can be used to throw exceptions within the context of procedure result entity binding.
/// </summary>
public interface IResultThrowHelper : IThrowHelper
{
    /// <summary>
    /// Creates a new throw helper for the specified column.
    /// </summary>
    /// <param name="property">The <see cref="PropertyInfo"/> representing the property being bound to the column.</param>
    /// <returns>The throw helper to be used for column configuration.</returns>
    IThrowHelper ForColumn(PropertyInfo property);
}
