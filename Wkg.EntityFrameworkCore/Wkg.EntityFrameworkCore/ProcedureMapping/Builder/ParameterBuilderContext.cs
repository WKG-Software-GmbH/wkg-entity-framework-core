using System.Reflection;
using Wkg.EntityFrameworkCore.ProcedureMapping.Builder.ThrowHelpers;

namespace Wkg.EntityFrameworkCore.ProcedureMapping.Builder;

public record ParameterBuilderContext(PropertyInfo PropertyInfo, IThrowHelper ThrowHelper);
