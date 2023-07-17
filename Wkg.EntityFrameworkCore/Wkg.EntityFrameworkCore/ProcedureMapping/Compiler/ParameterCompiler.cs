using Wkg.EntityFrameworkCore.ProcedureMapping.Builder;
using Wkg.EntityFrameworkCore.ProcedureMapping.Compiler.AccessorFactory;
using Wkg.EntityFrameworkCore.ProcedureMapping.Compiler.Output;

namespace Wkg.EntityFrameworkCore.ProcedureMapping.Compiler;

/// <summary>
/// Represents a compiler capable of translating a parameter builder into the IL code necessary to load the parameter from the I/O container and store results after the ADO.NET call.
/// </summary>
/// <typeparam name="TCompiledParameter"></typeparam>
public interface IParameterCompiler<out TCompiledParameter> where TCompiledParameter : struct, ICompiledParameter
{
    /// <summary>
    /// Compiles the parameter builder into the IL code necessary to load the parameter from the I/O container and store results after the ADO.NET call.
    /// </summary>
    public TCompiledParameter Compile();
}

/// <summary>
/// Represents the base class for a compiler capable of translating a parameter builder of type <typeparamref name="TBuilder"/> into the IL code necessary to load the parameter from the I/O container and store results after the ADO.NET call.
/// </summary>
/// <typeparam name="TBuilder">The concrete type of the parameter builder.</typeparam>
public abstract class ParameterCompiler<TBuilder> where TBuilder : IParameterBuilder
{
    /// <summary>
    /// The accessor builder used to generate the getter and setter for the parameter.
    /// </summary>
    protected IAccessorBuilder AccessorBuilder { get; }

    /// <summary>
    /// The parameter builder holding the configuration of the parameter.
    /// </summary>
    protected TBuilder Builder { get; }

    /// <summary>
    /// Creates a new instance of <see cref="ParameterCompiler{TBuilder}"/>.
    /// </summary>
    /// <param name="builder">The parameter builder holding the configuration of the parameter.</param>
    protected ParameterCompiler(TBuilder builder)
    {
        Builder = builder;
        AccessorBuilder = ILAccessorBuilder.CreateBuilder(Builder.Context.PropertyInfo, Builder.Context.ThrowHelper);
    }

    /// <summary>
    /// Generates the getter for the parameter.
    /// </summary>
    protected virtual PropertyGetter CreateGetter() => AccessorBuilder.BuildGetter();

    /// <summary>
    /// Generates the setter for the parameter, if the parameter is an output parameter. Otherwise, returns <see langword="null"/>.
    /// </summary>
    protected virtual PropertySetter? CreateSetter() => Builder.IsOutput ? AccessorBuilder.BuildSetter() : null;
}
