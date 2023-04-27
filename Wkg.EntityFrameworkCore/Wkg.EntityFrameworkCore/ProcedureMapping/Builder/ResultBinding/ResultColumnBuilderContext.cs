using System.Reflection;
using Wkg.EntityFrameworkCore.ProcedureMapping.Builder.ThrowHelpers;

namespace Wkg.EntityFrameworkCore.ProcedureMapping.Builder.ResultBinding;

public record ResultColumnBuilderContext(PropertyInfo ResultProperty, IThrowHelper ThrowHelper);
