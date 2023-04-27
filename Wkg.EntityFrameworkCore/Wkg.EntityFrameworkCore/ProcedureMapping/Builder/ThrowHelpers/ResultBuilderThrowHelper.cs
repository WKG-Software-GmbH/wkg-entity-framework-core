using System.Reflection;

namespace Wkg.EntityFrameworkCore.ProcedureMapping.Builder.ThrowHelpers;

internal class ResultBuilderThrowHelper : ThrowHelperBase, IResultThrowHelper
{
    protected override string TargetSite { get; }

    private readonly Type _result;
    private readonly Type _procedure;

    public ResultBuilderThrowHelper(Type procedure, Type result)
    {
        _procedure = procedure;
        _result = result;
        TargetSite = $"Result '{result.Name}' in procedure or function '{procedure.Name}'";
    }

    IThrowHelper IResultThrowHelper.ForColumn(PropertyInfo property) => 
        new ResultColumnBuilderThrowHelper(_procedure, _result, property);
}