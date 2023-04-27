using System.Reflection;

namespace Wkg.EntityFrameworkCore.ProcedureMapping.Builder.ThrowHelpers;

internal class ProcedureBuilderThrowHelper<TProcedure> : ThrowHelperBase, IProcedureThrowHelper
{
    protected override string TargetSite { get; } = $"In procedure or function '{typeof(TProcedure).Name}'";

    public IResultThrowHelper ForResult<TResult>() =>
        new ResultBuilderThrowHelper(typeof(TProcedure), typeof(TResult));

    IThrowHelper IProcedureThrowHelper.ForParameter(PropertyInfo property) => 
        new ParameterBuilderThrowHelper(typeof(TProcedure), property);
}