using System.Reflection;

namespace Wkg.EntityFrameworkCore.ProcedureMapping.Builder;

/// <summary>
/// Represents the required context in which an <see cref="IParameterBuilder"/> operates.
/// </summary>
/// <param name="PropertyInfo">The <see cref="PropertyInfo"/> of the property to be configured.</param>
/// <param name="ThrowHelper">The <see cref="IThrowHelper"/> to be used for throwing exceptions during parameter configuration.</param>
public record ParameterBuilderContext(PropertyInfo PropertyInfo, IThrowHelper ThrowHelper);
