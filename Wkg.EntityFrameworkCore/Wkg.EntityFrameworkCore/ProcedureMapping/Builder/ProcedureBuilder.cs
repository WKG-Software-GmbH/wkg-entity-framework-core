using System.Data;
using System.Data.Common;
using System.Diagnostics.CodeAnalysis;
using Wkg.EntityFrameworkCore.ProcedureMapping.Builder.ResultBinding;
using Wkg.EntityFrameworkCore.ProcedureMapping.Builder.ThrowHelpers;
using Wkg.EntityFrameworkCore.ProcedureMapping.Compiler;
using Wkg.EntityFrameworkCore.ProcedureMapping.Compiler.Output;
using Wkg.Common.Extensions;
using Wkg.Extensions.Reflection;

namespace Wkg.EntityFrameworkCore.ProcedureMapping.Builder;

/// <summary>
/// Provides a simple API for configuring a stored database procedure.
/// </summary>
public interface IProcedureBuilder
{
    /// <summary>
    /// The name of the procedure being mapped.
    /// </summary>
    string? ProcedureName { get; }

    /// <summary>
    /// Indicates whether the procedure is a database function.
    /// </summary>
    bool IsFunction { get; }
}

/// <summary>
/// Provides a simple API for configuring a stored database procedure.
/// </summary>
/// <typeparam name="TCompiledParameter">The concrete type of the compiled parameter.</typeparam>
/// <typeparam name="TDataReader">The concrete type of the <see cref="DbDataReader"/> to be used.</typeparam>
public interface IProcedureBuilder<TCompiledParameter, TDataReader> : IProcedureBuilder 
    where TCompiledParameter : struct, ICompiledParameter
    where TDataReader : DbDataReader
{
    /// <summary>
    /// Builds this stored procedure builder into an <see cref="IProcedureCompiler{TCompiledParameter}"/> instance that can be used to emit the runtime representation of the stored procedure.
    /// </summary>
    internal IProcedureCompiler<TCompiledParameter> Build();

    /// <summary>
    /// The <see cref="IParameterBuilder{TCompiledParameter}"/> instances used to configure the parameters of this stored procedure.
    /// </summary>
    internal IReadOnlyCollection<IParameterBuilder<TCompiledParameter>> ParameterBuilders { get; }

    /// <summary>
    /// The <see cref="IResultBuilder{TDataReader}"/> instance used to configure the set of result entities returned by this stored procedure.
    /// </summary>
    internal IResultBuilder<TDataReader>? ResultBuilder { get; }
}

/// <summary>
/// Provides a simple API for configuring a stored database procedure.
/// </summary>
/// <typeparam name="TProcedure">The concrete type of the stored procedure command object that represents the stored database procedure.</typeparam>
/// <typeparam name="TIOContainer">The type of the Input/Output container object used to pass arguments to and from the stored procedure.</typeparam>
/// <typeparam name="TCompiledParameter">The concrete type of the compiled parameter.</typeparam>
/// <typeparam name="TDataReader">The concrete type of the <see cref="DbDataReader"/> to be used.</typeparam>
/// <typeparam name="TProcedureBuilderImpl">The concrete type of the procedure builder implementation.</typeparam>
public abstract class ProcedureBuilder<TProcedure, TIOContainer, TCompiledParameter, TDataReader, TProcedureBuilderImpl> : IProcedureBuilder<TCompiledParameter, TDataReader>
    where TProcedure : class, IStoredProcedure
    where TIOContainer : class
    where TCompiledParameter : struct, ICompiledParameter
    where TDataReader : DbDataReader
    where TProcedureBuilderImpl : ProcedureBuilder<TProcedure, TIOContainer, TCompiledParameter, TDataReader, TProcedureBuilderImpl>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ProcedureBuilder{TProcedure, TIOContainer, TCompiledParameter, TDataReader, TProcedureBuilderImpl}"/> class.
    /// </summary>
    /// <exception cref="InvalidOperationException">Thrown when the generic type argument <typeparamref name="TProcedureBuilderImpl"/> does not match the concrete type of the derived class.</exception>
    protected ProcedureBuilder()
    {
        if (this is not TProcedureBuilderImpl)
        {
            ThrowHelper.Throw<InvalidOperationException>($"Expected {GetType().Name} as generic type argument {nameof(TProcedureBuilderImpl)} in {GetType().Name} but got {typeof(TProcedureBuilderImpl).Name}.");
        }
    }

    /// <summary>
    /// The <see cref="IThrowHelper"/> instance used to throw exceptions within the context of this procedure builder.
    /// </summary>
    protected IProcedureThrowHelper ThrowHelper { get; } = new ProcedureBuilderThrowHelper<TProcedure>();

    /// <inheritdoc cref="IProcedureBuilder.ProcedureName"/>
    protected string? ProcedureName { get; private set; }

    string? IProcedureBuilder.ProcedureName => ProcedureName;

    /// <inheritdoc cref="IProcedureBuilder.IsFunction"/>
    protected bool IsFunctionValue { get; private set; } = false;

    bool IProcedureBuilder.IsFunction => IsFunctionValue;

    /// <inheritdoc cref="IProcedureBuilder{TCompiledParameter, TDataReader}.ParameterBuilders"/>
    protected List<IParameterBuilder<TCompiledParameter>> ParameterBuilders { get; } = [];

    IReadOnlyCollection<IParameterBuilder<TCompiledParameter>> IProcedureBuilder<TCompiledParameter, TDataReader>.ParameterBuilders => ParameterBuilders;

    /// <inheritdoc cref="IProcedureBuilder{TCompiledParameter, TDataReader}.ResultBuilder"/>
    protected IResultBuilder<TDataReader>? ResultBuilder { get; set; }

    IResultBuilder<TDataReader>? IProcedureBuilder<TCompiledParameter, TDataReader>.ResultBuilder => ResultBuilder;

    /// <summary>
    /// Configures the stored procedure that the <typeparamref name="TProcedure"/> command object maps to.
    /// </summary>
    /// <param name="name">The name of the stored procedure.</param>
    /// <returns>The current builder instance.</returns>
    public TProcedureBuilderImpl ToDatabaseProcedure(string name)
    {
        if (ProcedureName is not null)
        {
            ThrowHelper.Throw<InvalidOperationException>($"Attempted to set multiple values for procedure name.");
        }
        ProcedureName = name;
        return this.To<TProcedureBuilderImpl>();
    }

    /// <summary>
    /// Configures the database function that the <typeparamref name="TProcedure"/> command object maps to.
    /// </summary>
    /// <param name="name">The name of the database function.</param>
    /// <returns>The current builder instance.</returns>
    public TProcedureBuilderImpl ToDatabaseFunction(string name)
    {
        if (ProcedureName is not null)
        {
            ThrowHelper.Throw<InvalidOperationException>($"Attempted to set multiple values for procedure name.");
        }
        ProcedureName = name;
        IsFunctionValue = true;
        return this.To<TProcedureBuilderImpl>();
    }

    /// <summary>
    /// Configures the stored procedure to be invoked as a database function.
    /// </summary>
    /// <param name="isFunction"><see langword="true"/> if the stored procedure is a database function; otherwise, <see langword="false"/>.</param>
    /// <returns>The current builder instance.</returns>
    public TProcedureBuilderImpl IsFunction(bool isFunction = true)
    {
        IsFunctionValue = isFunction;
        return this.To<TProcedureBuilderImpl>();
    }

    /// <summary>
    /// Asserts that the current builder instance is valid and that all required information has been provided.
    /// </summary>
    /// <exception cref="ArgumentNullException">Thrown when no procedure name has been provided.</exception>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when the procedure has more than one <see cref="ParameterDirection.ReturnValue"/> parameter.</exception>
    /// <exception cref="InvalidOperationException">Thrown when contradictory information has been provided.</exception>
    [MemberNotNull(nameof(ProcedureName))]
    protected virtual void AssertIsValid()
    {
        _ = ProcedureName ?? ThrowHelper.Throw<ArgumentNullException, string>("No procedure name has been provided!", nameof(ProcedureName));
        if (ParameterBuilders.Count(param => param.ParameterDirection is ParameterDirection.ReturnValue) > 1)
        {
            ThrowHelper.Throw<ArgumentOutOfRangeException>($"Procedure or function cannot have more than one {ParameterDirection.ReturnValue} parameter!", string
                .Join(',', ParameterBuilders
                    .Where(p => p.ParameterDirection is ParameterDirection.ReturnValue)
                    .Select(p => p.ParameterName)));
        }
        if (ResultBuilder is not null)
        {
            if (IsFunctionValue)
            {
                ThrowHelper.Throw<InvalidOperationException>($"A function cannot have a result set!");
            }
            if (ParameterBuilders.Any(param => param.ParameterDirection is ParameterDirection.ReturnValue))
            {
                ThrowHelper.Throw<InvalidOperationException>($"Procedure cannot have both a {ParameterDirection.ReturnValue} parameter and a result set!");
            }
            if (!typeof(TProcedure).ExtendsGenericBaseClass(typeof(StoredProcedure<,>)))
            {
                ThrowHelper.Throw<InvalidOperationException>($"In order to use a result set, the procedure must be derived from 'StoredProcedure<TProcedure, TResult>'!");
            }
            if ((typeof(TProcedure).GetGenericBaseClassTypeArguments(typeof(StoredProcedure<,>))?[1]) != ResultBuilder.ResultClrType)
            {
                ThrowHelper.Throw<InvalidOperationException>($"The result type of the procedure must be the same as the result type of the result set!");
            }
        }
        else if (typeof(TProcedure).ExtendsGenericBaseClass(typeof(StoredProcedure<,>)))
        {
            ThrowHelper.Throw<InvalidOperationException>($"The procedure inherits from 'StoredProcedure<TProcedure, TResult>' but no result set has been configured!");
        }
    }

    /// <inheritdoc cref="IProcedureBuilder{TCompiledParameter, TDataReader}.Build"/>
    protected abstract IProcedureCompiler<TCompiledParameter> Build();

    IProcedureCompiler<TCompiledParameter> IProcedureBuilder<TCompiledParameter, TDataReader>.Build()
    {
        AssertIsValid();
        return Build();
    }
}