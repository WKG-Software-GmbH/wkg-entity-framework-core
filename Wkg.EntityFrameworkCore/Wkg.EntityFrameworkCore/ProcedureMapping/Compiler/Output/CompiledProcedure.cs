using Wkg.EntityFrameworkCore.ProcedureMapping.Runtime;

namespace Wkg.EntityFrameworkCore.ProcedureMapping.Compiler.Output;

public interface ICompiledProcedure
{
    IProcedureExecutionContext CreateExecutionContext();

    internal Type ProcedureType { get; }
}

public sealed class CompiledProcedure<TCompiledParameter> : ICompiledProcedure where TCompiledParameter : struct, ICompiledParameter
{
    private readonly TCompiledParameter[] _compiledParameters;
    internal string ProcedureName { get; }
    internal bool IsFunction { get; }
    internal ReadOnlySpan<TCompiledParameter> CompiledParameters => new(_compiledParameters);
    internal int ParameterCount => _compiledParameters.Length;
    internal CompiledResult? CompiledResult { get; } 
    Type ICompiledProcedure.ProcedureType => _procedureType;

    private readonly Type _procedureType;

    public CompiledProcedure(string procedureName, bool isFunction, TCompiledParameter[] parameters, Type procedureType, CompiledResult? compiledResult)
    {
        _compiledParameters = parameters;
        ProcedureName = procedureName;
        IsFunction = isFunction;
        _procedureType = procedureType;
        CompiledResult = compiledResult;
    }

    public IProcedureExecutionContext CreateExecutionContext() => new ProcedureExecutionContext<TCompiledParameter>(this);
}
