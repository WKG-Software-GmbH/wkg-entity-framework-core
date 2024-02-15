using System.Data.Common;
using Wkg.EntityFrameworkCore.ProcedureMapping.Builder.ThrowHelpers;
using Wkg.EntityFrameworkCore.ProcedureMapping.Compiler.ResultBinding;
using Wkg.Common.Extensions;

namespace Wkg.EntityFrameworkCore.ProcedureMapping.Builder.ResultBinding;

/// <summary>
/// Represents a builder for result entities of a stored procedure.
/// </summary>
public interface IResultBuilder
{
    /// <summary>
    /// Indicates whether the result is a collection or a single entity.
    /// </summary>
    bool IsCollection { get; }

    internal Type ResultClrType { get; }

    /// <summary>
    /// The <see cref="IThrowHelper"/> to be used when an error is encountered.
    /// </summary>
    IThrowHelper ThrowHelper { get; }
}

/// <summary>
/// Represents a builder for result entities of a stored procedure targeting a specific <see cref="DbDataReader"/> of type <typeparamref name="TDataReader"/>.
/// </summary>
/// <typeparam name="TDataReader">The type of the <see cref="DbDataReader"/> to be used.</typeparam>
public interface IResultBuilder<TDataReader> : IResultBuilder where TDataReader : DbDataReader
{
    internal IReadOnlyCollection<IResultColumnBuilder> ColumnBuilders { get; }

    internal IResultCompiler<TDataReader> Build();
}

/// <summary>
/// The base class for all result entity builders for result type <typeparamref name="TResult"/> of a stored procedure targeting a specific <see cref="DbDataReader"/> of type <typeparamref name="TDataReader"/>.
/// </summary>
/// <typeparam name="TResult">The type of the result collection.</typeparam>
/// <typeparam name="TDataReader">The type of the <see cref="DbDataReader"/> to be used.</typeparam>
/// <typeparam name="TResultBuilderImpl">The concrete implementation of the <see cref="ResultBuilder{TResult, TDataReader, TResultBuilderImpl}"/>.</typeparam>
public abstract class ResultBuilder<TResult, TDataReader, TResultBuilderImpl> : IResultBuilder<TDataReader>
    where TResult : class
    where TDataReader : DbDataReader
    where TResultBuilderImpl : ResultBuilder<TResult, TDataReader, TResultBuilderImpl>
{
    /// <summary>
    /// The <see cref="IThrowHelper"/> to be used when an error is encountered.
    /// </summary>
    protected IResultThrowHelper ThrowHelper { get; }

    IThrowHelper IResultBuilder.ThrowHelper => ThrowHelper;

    /// <summary>
    /// Indicates whether the result is a collection or a single entity.
    /// </summary>
    protected bool IsCollection { get; private set; }

    bool IResultBuilder.IsCollection => IsCollection;

    /// <summary>
    /// The <see cref="IResultColumnBuilder"/> collection to be used when building the result from the <typeparamref name="TDataReader"/>.
    /// </summary>
    protected List<IResultColumnBuilder> ColumnBuilders { get; } = [];

    IReadOnlyCollection<IResultColumnBuilder> IResultBuilder<TDataReader>.ColumnBuilders => ColumnBuilders;

    /// <summary>
    /// The CLR type of the result entity.
    /// </summary>
    protected Type ResultClrType { get; }

    Type IResultBuilder.ResultClrType => ResultClrType;

    /// <summary>
    /// Initializes a new instance of the <see cref="ResultBuilder{TResult, TDataReader, TResultBuilderImpl}"/> class.
    /// </summary>
    /// <param name="throwHelper">The <see cref="IThrowHelper"/> to be used when an error is encountered.</param>
    /// <param name="clrType">The CLR type of the result entity.</param>
    protected ResultBuilder(IProcedureThrowHelper throwHelper, Type clrType)
    {
        if (this is not TResultBuilderImpl)
        {
            throwHelper.Throw<InvalidOperationException>($"Expected {GetType().Name} as generic type argument {nameof(TResultBuilderImpl)} in {GetType().Name} but got {typeof(TResultBuilderImpl).Name}.");
        }

        ThrowHelper = throwHelper.ForResult<TResult>();
        IsCollection = true;
        ResultClrType = clrType;
    }

    /// <summary>
    /// Configures the stored procedure to return a collection of result entities (multiple rows).
    /// </summary>
    /// <returns>The current builder instance.</returns>
    public TResultBuilderImpl AsCollection()
    {
        IsCollection = true;
        return this.To<TResultBuilderImpl>();
    }

    /// <summary>
    /// Configures the stored procedure to return a single result entity (single row).
    /// </summary>
    public TResultBuilderImpl AsSingle()
    {
        IsCollection = false;
        return this.To<TResultBuilderImpl>();
    }

    /// <summary>
    /// Builds this instance into a <see cref="IResultCompiler{TDataReader}"/> that can be used to compile all required bindings and conversions.
    /// </summary>
    protected abstract IResultCompiler<TDataReader> Build();

    IResultCompiler<TDataReader> IResultBuilder<TDataReader>.Build() => Build();
}
