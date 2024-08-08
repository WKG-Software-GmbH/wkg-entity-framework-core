using System.Data.Common;
using System.Linq.Expressions;
using Wkg.EntityFrameworkCore.ProcedureMapping.Compiler.ResultBinding;
using Wkg.Common.Extensions;

namespace Wkg.EntityFrameworkCore.ProcedureMapping.Builder.ResultBinding;

/// <summary>
/// Represents a type safe proxy builder for a corresponding <see cref="ResultColumnBuilder{TResult, TProperty, TProxiedBuilder}"/>.
/// </summary>
/// <remarks>
/// This class ensures that type read from the database is compatible with the conversion expression and the property type.
/// </remarks>
/// <typeparam name="TResult">The type of the result entity owning the column being configured.</typeparam>
/// <typeparam name="TProperty">The type of the property being mapped.</typeparam>
/// <typeparam name="TColumn">The CLR type of the column being mapped.</typeparam>
/// <typeparam name="TProxiedBuilder">The type of the proxied builder.</typeparam>
/// <typeparam name="TProxyImpl">The type of the proxy implementation.</typeparam>
/// <remarks>
/// Initializes a new instance of the <see cref="TypedResultColumnBuilderProxy{TResult, TProperty, TColumn, TProxiedBuilder, TProxyImpl}"/> class.
/// </remarks>
/// <param name="proxiedBuilder">The proxied builder.</param>
public abstract class TypedResultColumnBuilderProxy<TResult, TProperty, TColumn, TProxiedBuilder, TProxyImpl>(TProxiedBuilder proxiedBuilder) 
    : ResultColumnBuilderBase<TResult, TProperty, TProxyImpl>(proxiedBuilder.To<IResultColumnBuilder>().Context)
    where TProxiedBuilder : ResultColumnBuilder<TResult, TProperty, TProxiedBuilder>
    where TProxyImpl : TypedResultColumnBuilderProxy<TResult, TProperty, TColumn, TProxiedBuilder, TProxyImpl>
{
    /// <summary>
    /// The proxied builder.
    /// </summary>
    protected TProxiedBuilder ProxiedBuilder { get; } = proxiedBuilder;

    /// <inheritdoc/>
    public override TProxyImpl HasName(string name)
    {
        ProxiedBuilder.HasName(name);
        return this.To<TProxyImpl>();
    }

    /// <inheritdoc/>
    public override TProxyImpl MayBeNull()
    {
        ProxiedBuilder.MayBeNull();
        return this.To<TProxyImpl>();
    }

    /// <summary>
    /// Configures the conversion expression to be used to convert the column value read from the <see cref="DbDataReader"/> to the property type.
    /// </summary>
    /// <param name="conversion">The conversion expression.</param>
    /// <returns>The builder instance.</returns>
    public TProxyImpl RequiresConversion(Expression<Func<TColumn, TProperty>> conversion)
    {
        ProxiedBuilder.RequiresConversion(conversion);
        return this.To<TProxyImpl>();
    }

    /// <inheritdoc/>
    internal protected override string? ColumnName
    {
        get => ProxiedBuilder.ColumnName;
        set => ProxiedBuilder.ColumnName = value;
    }

    /// <inheritdoc/>
    internal protected override LambdaExpression? Conversion
    {
        get => ProxiedBuilder.Conversion;
        set => ProxiedBuilder.Conversion = value;
    }

    /// <inheritdoc/>
    public override IResultColumnCompilerHint? CompilerHint
    {
        get => ProxiedBuilder.CompilerHint;
        internal protected set => ProxiedBuilder.CompilerHint = value;
    }

    /// <inheritdoc/>
    /// <exception cref="NotSupportedException"/>
    protected override IResultColumnCompiler Build() => throw new NotSupportedException();
}