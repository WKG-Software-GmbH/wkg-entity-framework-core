using System.Reflection;

namespace Wkg.EntityFrameworkCore.ProcedureMapping.Builder.ThrowHelpers;

internal class ParameterBuilderThrowHelper(Type procedureType, PropertyInfo property) : ThrowHelperBase
{
    protected override string TargetSite { get; } = $"Parameter '{property.Name}' in procedure or function '{procedureType.Name}'";
}