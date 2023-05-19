using Microsoft.EntityFrameworkCore.Infrastructure;
using System.Data;
using System.Diagnostics.CodeAnalysis;
using System.Linq.Expressions;
using System.Reflection;
using Wkg.EntityFrameworkCore.ProcedureMapping.Builder.ThrowHelpers;
using Wkg.EntityFrameworkCore.ProcedureMapping.Compiler;
using Wkg.EntityFrameworkCore.ProcedureMapping.Compiler.Output;
using Wkg.Extensions.Common;

namespace Wkg.EntityFrameworkCore.ProcedureMapping.Builder;

/// <summary>
/// Provides a simple API for configuring a stored procedure parameter
/// </summary>
public interface IParameterBuilder
{
    /// <summary>
    /// The direction of the parameter.
    /// </summary>
    ParameterDirection ParameterDirection { get; }

    /// <summary>
    /// The name of the parameter.
    /// </summary>
    string? ParameterName { get; }

    /// <summary>
    /// Indicates whether the parameter will be written to during procedure execution.
    /// </summary>
    bool IsOutput { get; }

    /// <summary>
    /// Provides a hint for the expected size of the data within the parameter.
    /// </summary>
    int Size { get; }

    /// <summary>
    /// The context to be used by this parameter builder.
    /// </summary>
    ParameterBuilderContext Context { get; }
}

/// <summary>
/// Provides a simple API for configuring a stored procedure parameter.
/// </summary>
/// <typeparam name="TCompiledParameter">The concrete type of the compiled parameter.</typeparam>
public interface IParameterBuilder<out TCompiledParameter> : IParameterBuilder where TCompiledParameter : struct, ICompiledParameter
{
    internal IParameterCompiler<TCompiledParameter> Build();
}

/// <summary>
/// Provides a simple API for configuring a stored procedure parameter.
/// </summary>
/// <typeparam name="TIOContainer">The type of the Input/Output container object used to pass arguments to and from the stored procedure.</typeparam>
/// <typeparam name="TParameter">The CLR type of the parameter being mapped.</typeparam>
/// <typeparam name="TCompiledParameter">The concrete type of the compiled parameter.</typeparam>
/// <typeparam name="TParameterBuilderImpl">The concrete type of the implementing parameter builder.</typeparam>
public abstract class ParameterBuilder<TIOContainer, TParameter, TCompiledParameter, TParameterBuilderImpl> : IParameterBuilder<TCompiledParameter>
    where TIOContainer : class
    where TCompiledParameter : struct, ICompiledParameter
{
    /// <inheritdoc cref="IParameterBuilder.ParameterName"/>
    protected string ParameterName { get; set; }

    /// <inheritdoc cref="IParameterBuilder.Size"/>
    protected int Size { get; set; }

    /// <inheritdoc cref="IParameterBuilder.ParameterDirection"/>
    protected ParameterDirection ParameterDirection { get; private set; } = ParameterDirection.Input;

    bool IParameterBuilder.IsOutput => ParameterDirection is ParameterDirection.Output or ParameterDirection.InputOutput or ParameterDirection.ReturnValue;

    ParameterDirection IParameterBuilder.ParameterDirection => ParameterDirection;

    int IParameterBuilder.Size => Size;

    string? IParameterBuilder.ParameterName => ParameterName;

    ParameterBuilderContext IParameterBuilder.Context => Context;

    /// <inheritdoc cref="IParameterBuilder.Context"/>
    protected ParameterBuilderContext Context { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="ParameterBuilder{TIOContainer, TParameter, TCompiledParameter, TParameterBuilderImpl}"/> class.
    /// </summary>
    /// <param name="parameterSelector">A lambda expression representing the property of the <typeparamref name="TIOContainer"/> that will be configured (e.g. <c>io => io.Name</c>).</param>
    /// <param name="throwHelper">The throw helper to be used by this parameter builder.</param>
    /// <exception cref="InvalidOperationException">Thrown when the generic type argument <typeparamref name="TParameterBuilderImpl"/> does not match the type of the implementing class.</exception>
    /// <exception cref="ArgumentException">Thrown when the provided <paramref name="parameterSelector"/> does not select a property.</exception>
    protected ParameterBuilder(Expression<Func<TIOContainer, TParameter>> parameterSelector, IProcedureThrowHelper throwHelper)
    {
        if (this is not TParameterBuilderImpl)
        {
            throwHelper.Throw<InvalidOperationException>($"Expected {GetType().Name} as generic type argument {nameof(TParameterBuilderImpl)} in {GetType().Name} but got {typeof(TParameterBuilderImpl).Name}.");
        }

        PropertyInfo info = parameterSelector.GetMemberAccess() as PropertyInfo
            ?? throwHelper.Throw<ArgumentException, PropertyInfo>($"Property selector expression provided as {parameterSelector} must select property.");
        ParameterName = info.Name;
        IThrowHelper parameterThrowHelper = throwHelper.ForParameter(info);
        Context = new ParameterBuilderContext(info, throwHelper);
    }

    /// <summary>
    /// Configures the name of the parameter that the property maps to in the stored procedure.
    /// </summary>
    /// <param name="parameterName">The name of the parameter.</param>
    /// <returns>The current builder instance.</returns>
    public TParameterBuilderImpl HasName(string parameterName)
    {
        ParameterName = parameterName;
        return this.To<TParameterBuilderImpl>();
    }

    /// <summary>
    /// Configures the direction of the parameter.
    /// </summary>
    /// <param name="direction">The direction of the parameter.</param>
    /// <returns>The current builder instance.</returns>
    public TParameterBuilderImpl HasDirection(ParameterDirection direction)
    {
        ParameterDirection = direction;
        if (direction is ParameterDirection.ReturnValue && ParameterName is null)
        {
            ParameterName = $"ReturnValue_{Guid.NewGuid():N}";
        }
        return this.To<TParameterBuilderImpl>();
    }

    /// <summary>
    /// Configures the maximum length of data that the parameter can hold.
    /// </summary>
    /// <param name="size">The maximum length of data that the parameter can hold.</param>
    /// <returns>The current builder instance.</returns>
    public TParameterBuilderImpl HasSize(int size)
    {
        Size = size;
        return this.To<TParameterBuilderImpl>();
    }

    /// <summary>
    /// Returns a <see cref="IParameterCompiler{TCompiledParameter}"/> that can be used to emit the IL required to read and write the parameter.
    /// </summary>
    protected abstract IParameterCompiler<TCompiledParameter> Build();

    /// <summary>
    /// Asserts that the current state of the builder is valid and that all required information has been provided.
    /// </summary>
    /// <exception cref="ArgumentNullException">Thrown when <see cref="ParameterName"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when <see cref="Size"/> is less than zero.</exception>
    [MemberNotNull(nameof(ParameterName))]
    protected virtual void AssertIsValid()
    {
        _ = ParameterName ?? Context.ThrowHelper.Throw<ArgumentNullException, string>("Attempted to build parameter with no name!");

        if (Size < 0)
        {
            Context.ThrowHelper.Throw<ArgumentOutOfRangeException>($"Attempted to build parameter '{ParameterName}' with negative size ({Size})!");
        }
    }

    IParameterCompiler<TCompiledParameter> IParameterBuilder<TCompiledParameter>.Build()
    {
        AssertIsValid();
        return Build();
    }
}
