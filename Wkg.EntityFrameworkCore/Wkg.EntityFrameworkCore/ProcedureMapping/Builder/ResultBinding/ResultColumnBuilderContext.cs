using System.Reflection;

namespace Wkg.EntityFrameworkCore.ProcedureMapping.Builder.ResultBinding;

/// <summary>
/// Represents the required context in which an <see cref="IResultColumnBuilder"/> operates.
/// </summary>
/// <param name="ResultProperty">The property of the result type that is being configured.</param>
/// <param name="ThrowHelper">The throw helper to be used.</param>
public record ResultColumnBuilderContext(PropertyInfo ResultProperty, IThrowHelper ThrowHelper);
