using System.Reflection;

namespace Wkg.EntityFrameworkCore.ProcedureMapping.Builder.ThrowHelpers;

internal class ResultColumnBuilderThrowHelper(Type procedure, Type result, PropertyInfo property) : ThrowHelperBase
{
    protected override string TargetSite { get; } = $"Column '{property.Name}' of result '{result.Name}' in procedure or function '{procedure.Name}'";
}