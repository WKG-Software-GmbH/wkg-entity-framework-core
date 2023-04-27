using Wkg.EntityFrameworkCore.ProcedureMapping.Builder;
using Wkg.EntityFrameworkCore.ProcedureMapping.Compiler.Output;

namespace Wkg.EntityFrameworkCore.ProcedureMapping.Compiler;

public interface IProcedureCompiler<in TCompiledParameter> where TCompiledParameter : struct, ICompiledParameter
{
    ICompiledProcedure Compile(TCompiledParameter[] compiledParameters, CompiledResult? compiledResult);
}

public abstract class ProcedureCompiler<TBuilder> where TBuilder : IProcedureBuilder
{
    protected TBuilder Builder { get; }

    protected Type ProcedureType { get; }

    protected ProcedureCompiler(TBuilder builder, Type procedureType)
    {
        Builder = builder;
        ProcedureType = procedureType;
    }
}