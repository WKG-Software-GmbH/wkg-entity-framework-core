using Wkg.EntityFrameworkCore.ProcedureMapping.Builder;
using Wkg.EntityFrameworkCore.ProcedureMapping.Compiler.AccessorGeneration;
using Wkg.EntityFrameworkCore.ProcedureMapping.Compiler.Output;

namespace Wkg.EntityFrameworkCore.ProcedureMapping.Compiler;

public interface IParameterCompiler<out TCompiledParameter> where TCompiledParameter : struct, ICompiledParameter
{
    public TCompiledParameter Compile();
}

public abstract class ParameterCompiler<TBuilder> where TBuilder : IParameterBuilder
{
    protected IAccessorBuilder AccessorBuilder { get; }

    protected TBuilder Builder { get; }

    protected ParameterCompiler(TBuilder builder)
    {
        Builder = builder;
        AccessorBuilder = ILAccessorBuilder.CreateBuilder(Builder.Context.PropertyInfo, Builder.Context.ThrowHelper);
    }

    protected virtual PropertyGetter CreateGetter() => AccessorBuilder.BuildGetter();

    protected virtual PropertySetter? CreateSetter() => Builder.IsOutput ? AccessorBuilder.BuildSetter() : null;
}
