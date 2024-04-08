using System.Reflection;

namespace Wkg.EntityFrameworkCore.ProcedureMapping.Builder.ThrowHelpers;

internal class ResultBuilderThrowHelper(Type procedure, Type result) : ThrowHelperBase, IResultThrowHelper
{
    protected override string TargetSite { get; } = $"Result '{result.Name}' in procedure or function '{procedure.Name}'";

    IThrowHelper IResultThrowHelper.ForColumn(PropertyInfo property) => 
        new ResultColumnBuilderThrowHelper(procedure, result, property);
}