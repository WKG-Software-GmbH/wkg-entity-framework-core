using System.Linq.Expressions;
using Wkg.EntityFrameworkCore.ProcedureMapping.Compiler.ResultBinding;
using Wkg.Extensions.Common;

namespace Wkg.EntityFrameworkCore.ProcedureMapping.Builder.ResultBinding;

public abstract class TypedResultColumnBuilderProxy<TResult, TProperty, TColumn, TProxiedBuilder, TProxyImpl> : ResultColumnBuilderBase<TResult, TProperty, TProxyImpl>
    where TProxiedBuilder : ResultColumnBuilder<TResult, TProperty, TProxiedBuilder>
    where TProxyImpl : TypedResultColumnBuilderProxy<TResult, TProperty, TColumn, TProxiedBuilder, TProxyImpl>
{
    protected TProxiedBuilder ProxiedBuilder { get; }

    protected TypedResultColumnBuilderProxy(TProxiedBuilder proxiedBuilder) : base(proxiedBuilder.To<IResultColumnBuilder>().Context)
    {
        ProxiedBuilder = proxiedBuilder;
    }

    public override TProxyImpl HasName(string name)
    {
        ProxiedBuilder.HasName(name);
        return this.To<TProxyImpl>();
    }

    public override TProxyImpl MayBeNull()
    {
        ProxiedBuilder.MayBeNull();
        return this.To<TProxyImpl>();
    }

    public TProxyImpl RequiresConversion(Expression<Func<TColumn, TProperty>> conversion)
    {
        ProxiedBuilder.RequiresConversion(conversion);
        return this.To<TProxyImpl>();
    }

    protected internal override string? ColumnName
    {
        get => ProxiedBuilder.ColumnName;
        set => ProxiedBuilder.ColumnName = value;
    }

    protected internal override LambdaExpression? Conversion
    {
        get => ProxiedBuilder.Conversion;
        set => ProxiedBuilder.Conversion = value;
    }

    public override IResultColumnCompilerHint? CompilerHint
    {
        get => ProxiedBuilder.CompilerHint;
        protected internal set => ProxiedBuilder.CompilerHint = value;
    }

    protected override IResultColumnCompiler Build() => throw new NotSupportedException();
}