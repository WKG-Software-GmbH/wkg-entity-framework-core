using System.Reflection;

namespace Wkg.EntityFrameworkCore.ProcedureMapping.Builder.ThrowHelpers;

internal class ResultColumnBuilderThrowHelper : ThrowHelperBase
{
    protected override string TargetSite { get; }
    
    public ResultColumnBuilderThrowHelper(Type procedure, Type result, PropertyInfo property) =>
        TargetSite = $"Column '{property.Name}' of result '{result.Name}' in procedure or function '{procedure.Name}'";
}