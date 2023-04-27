using System.Reflection;

namespace Wkg.EntityFrameworkCore.ProcedureMapping.Builder.ThrowHelpers;

internal class ParameterBuilderThrowHelper : ThrowHelperBase
{
    protected override string TargetSite { get; }
    
    public ParameterBuilderThrowHelper(Type procedureType, PropertyInfo property) =>
        TargetSite = $"Parameter '{property.Name}' in procedure or function '{procedureType.Name}'";
}