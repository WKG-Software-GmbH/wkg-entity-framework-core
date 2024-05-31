using Microsoft.EntityFrameworkCore.Infrastructure;
using System.Data.Common;
using System.Diagnostics.CodeAnalysis;
using System.Linq.Expressions;
using System.Reflection;
using Wkg.EntityFrameworkCore.ProcedureMapping.Builder.ThrowHelpers;
using Wkg.EntityFrameworkCore.ProcedureMapping.Compiler.ResultBinding;
using Wkg.Common.Extensions;

namespace Wkg.EntityFrameworkCore.ProcedureMapping.Builder.ResultBinding;

/// <summary>
/// Represents a builder for a result column.
/// </summary>
public interface IResultColumnBuilder
{
    /// <summary>
    /// The name of the column configured by this builder.
    /// </summary>
    string? ColumnName { get; }

    /// <summary>
    /// The conversion expression that will be applied to the column read from the <see cref="DbDataReader"/> before it is assigned to the property.
    /// </summary>
    LambdaExpression? Conversion { get; }

    /// <summary>
    /// The context in which this builder is being used.
    /// </summary>
    ResultColumnBuilderContext Context { get; }

    /// <summary>
    /// Indicates whether the column can contain null values.
    /// </summary>
    bool IsNullable { get; }

    internal IResultColumnCompiler Build();

    /// <summary>
    /// The compiler hint to be considered when compiling the bindings for this column.
    /// </summary>
    IResultColumnCompilerHint? CompilerHint { get; }
}

/// <summary>
/// The base class for all result column builders.
/// </summary>
/// <typeparam name="TResult">The type of the result entity being configured.</typeparam>
/// <typeparam name="TProperty">The type of the property being configured.</typeparam>
/// <typeparam name="TResultColumnBuilderImpl">The type of the implementing builder.</typeparam>
public abstract class ResultColumnBuilderBase<TResult, TProperty, TResultColumnBuilderImpl> : IResultColumnBuilder
    where TResultColumnBuilderImpl : ResultColumnBuilderBase<TResult, TProperty, TResultColumnBuilderImpl>
{
    private IResultColumnCompilerHint? _compilerHint;

    /// <inheritdoc cref="IResultColumnBuilder.CompilerHint"/>
    public virtual IResultColumnCompilerHint? CompilerHint
    {
        get => _compilerHint;
        protected internal set
        {
            if (CompilerHint is not null)
            {
                Context.ThrowHelper.Throw<InvalidOperationException>("Attempted to set compiler hint on a column that already has a hint! This was unexpected at this time.");
            }
            _compilerHint = value;
        }
    }

    IResultColumnCompilerHint? IResultColumnBuilder.CompilerHint { get; }

    /// <inheritdoc cref="IResultColumnBuilder.ColumnName"/>
    protected internal virtual string? ColumnName { get; set; }

    string? IResultColumnBuilder.ColumnName => ColumnName;

    /// <inheritdoc cref="IResultColumnBuilder.Conversion"/>
    protected internal virtual LambdaExpression? Conversion { get; set; }

    LambdaExpression? IResultColumnBuilder.Conversion => Conversion;

    /// <inheritdoc cref="IResultColumnBuilder.Context"/>
    protected virtual ResultColumnBuilderContext Context { get; }

    ResultColumnBuilderContext IResultColumnBuilder.Context => Context;

    /// <inheritdoc cref="IResultColumnBuilder.IsNullable"/>
    protected internal virtual bool IsNullable { get; set; }

    bool IResultColumnBuilder.IsNullable => IsNullable;

    /// <summary>
    /// Creates a new instance of the <see cref="ResultColumnBuilderBase{TResult, TProperty, TResultColumnBuilderImpl}"/> class.
    /// </summary>
    /// <param name="columnSelector">The column selector expression.</param>
    /// <param name="throwHelper">The throw helper to be used when an exception is encountered.</param>
    /// <exception cref="InvalidOperationException"/>
    /// <exception cref="ArgumentException"/>
    protected ResultColumnBuilderBase(Expression<Func<TResult, TProperty>> columnSelector, IResultThrowHelper throwHelper)
    {
        if (this is not TResultColumnBuilderImpl)
        {
            throwHelper.Throw<InvalidOperationException>($"Expected {GetType().Name} as generic type argument {nameof(TResultColumnBuilderImpl)} in {GetType().Name} but got {typeof(TResultColumnBuilderImpl).Name}.");
        }

        PropertyInfo info = columnSelector.GetMemberAccess() as PropertyInfo
            ?? throwHelper.Throw<ArgumentException, PropertyInfo>($"Column selector expression provided as {columnSelector} must select property.");

        IThrowHelper columnThrowHelper = throwHelper.ForColumn(info);
        Context = new ResultColumnBuilderContext(info, columnThrowHelper);
    }

    private protected ResultColumnBuilderBase(ResultColumnBuilderContext context)
    {
        Context = context;
    }

    /// <summary>
    /// Configures the name of the column in the result set returned by the database procedure.
    /// </summary>
    /// <param name="name">The name of the column.</param>
    /// <returns>The current instance of the builder.</returns>
    public virtual TResultColumnBuilderImpl HasName(string name)
    {
        ColumnName = name;
        return this.To<TResultColumnBuilderImpl>();
    }

    /// <summary>
    /// Configures whether this property must have a value assigned to it or whether <see langword="null"/> is a valid value.
    /// </summary>
    /// <returns>The current instance of the builder.</returns>
    public virtual TResultColumnBuilderImpl MayBeNull()
    {
        IsNullable = true;
        return this.To<TResultColumnBuilderImpl>();
    }

    /// <summary>
    /// Asserts that the current configuration is valid and that all required information has been provided.
    /// </summary>
    /// <exception cref="ArgumentNullException"/>
    [MemberNotNull(nameof(ColumnName))]
    protected virtual void AssertIsValid() => 
        _ = ColumnName ?? Context.ThrowHelper.Throw<ArgumentNullException, string>("Attempted to build column without name!", nameof(ColumnName));

    /// <summary>
    /// Attempts to auto-configure the column based on the information available in the context.
    /// </summary>
    protected abstract void AttemptAutoConfiguration();

    /// <summary>
    /// Transforms this builder instance into a <see cref="IResultColumnCompiler"/> that can be used to emit the column bindings.
    /// </summary>
    protected abstract IResultColumnCompiler Build();

    IResultColumnCompiler IResultColumnBuilder.Build()
    {
        AttemptAutoConfiguration();
        AssertIsValid();
        return Build();
    }
}

/// <summary>
/// The base class for all non-proxied result column builders.
/// </summary>
/// <inheritdoc/>
/// <remarks>
/// Creates a new instance of the <see cref="ResultColumnBuilder{TResult, TProperty, TResultColumnBuilderImpl}"/> class.
/// </remarks>
/// <inheritdoc/>
public abstract class ResultColumnBuilder<TResult, TProperty, TResultColumnBuilderImpl>(Expression<Func<TResult, TProperty>> columnSelector, IResultThrowHelper throwHelper) 
    : ResultColumnBuilderBase<TResult, TProperty, TResultColumnBuilderImpl>(columnSelector, throwHelper)
    where TResultColumnBuilderImpl : ResultColumnBuilder<TResult, TProperty, TResultColumnBuilderImpl>
{

    /// <summary>
    /// Configures the conversion to be applied after the column value has been read from the <see cref="DbDataReader"/> but before it is assigned to the property.
    /// </summary>
    /// <typeparam name="TColumn">The type of the column in the <see cref="DbDataReader"/> result set.</typeparam>
    /// <param name="conversion">The expression to convert values of type <typeparamref name="TColumn"/> to <typeparamref name="TProperty"/>.</param>
    /// <returns>The current instance of the builder.</returns>
    public virtual TResultColumnBuilderImpl RequiresConversion<TColumn>(Expression<Func<TColumn, TProperty>> conversion)
    {
        Conversion = conversion;
        return this.To<TResultColumnBuilderImpl>();
    }
}