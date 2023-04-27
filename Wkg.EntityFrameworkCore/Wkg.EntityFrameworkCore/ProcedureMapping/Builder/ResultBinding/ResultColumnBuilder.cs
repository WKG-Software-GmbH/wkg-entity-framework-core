using Microsoft.EntityFrameworkCore.Infrastructure;
using System.Diagnostics.CodeAnalysis;
using System.Linq.Expressions;
using System.Reflection;
using Wkg.EntityFrameworkCore.ProcedureMapping.Builder.ThrowHelpers;
using Wkg.EntityFrameworkCore.ProcedureMapping.Compiler.ResultBinding;
using Wkg.Extensions.Common;

namespace Wkg.EntityFrameworkCore.ProcedureMapping.Builder.ResultBinding;

public interface IResultColumnBuilder
{
    string? ColumnName { get; }

    LambdaExpression? Conversion { get; }

    ResultColumnBuilderContext Context { get; }

    bool IsNullable { get; }

    internal IResultColumnCompiler Build();

    IResultColumnCompilerHint? CompilerHint { get; }
}

public abstract class ResultColumnBuilderBase<TResult, TProperty, TResultColumnBuilderImpl> : IResultColumnBuilder
    where TResultColumnBuilderImpl : ResultColumnBuilderBase<TResult, TProperty, TResultColumnBuilderImpl>
{
    private IResultColumnCompilerHint? _compilerHint;

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

    protected internal virtual string? ColumnName { get; set; }

    string? IResultColumnBuilder.ColumnName => ColumnName;

    protected internal virtual LambdaExpression? Conversion { get; set; }

    LambdaExpression? IResultColumnBuilder.Conversion => Conversion;

    protected virtual ResultColumnBuilderContext Context { get; }

    ResultColumnBuilderContext IResultColumnBuilder.Context => Context;

    protected internal virtual bool IsNullable { get; set; }

    bool IResultColumnBuilder.IsNullable => IsNullable;

    public ResultColumnBuilderBase(Expression<Func<TResult, TProperty>> columnSelector, IResultThrowHelper throwHelper)
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

    public virtual TResultColumnBuilderImpl HasName(string name)
    {
        ColumnName = name;
        return this.To<TResultColumnBuilderImpl>();
    }

    public virtual TResultColumnBuilderImpl MayBeNull()
    {
        IsNullable = true;
        return this.To<TResultColumnBuilderImpl>();
    }

    [MemberNotNull(nameof(ColumnName))]
    protected virtual void AssertIsValid()
    {
        _ = ColumnName ?? Context.ThrowHelper.Throw<ArgumentNullException, string>("Attempted to build column without name!", nameof(ColumnName));
    }

    protected abstract void AttemptAutoConfiguration();

    protected abstract IResultColumnCompiler Build();

    IResultColumnCompiler IResultColumnBuilder.Build()
    {
        AttemptAutoConfiguration();
        AssertIsValid();
        return Build();
    }
}

public abstract class ResultColumnBuilder<TResult, TProperty, TResultColumnBuilderImpl> : ResultColumnBuilderBase<TResult, TProperty, TResultColumnBuilderImpl>
    where TResultColumnBuilderImpl : ResultColumnBuilder<TResult, TProperty, TResultColumnBuilderImpl>
{
    public ResultColumnBuilder(Expression<Func<TResult, TProperty>> columnSelector, IResultThrowHelper throwHelper) : base(columnSelector, throwHelper)
    {
    }

    public virtual TResultColumnBuilderImpl RequiresConversion<TColumn>(Expression<Func<TColumn, TProperty>> conversion)
    {
        Conversion = conversion;
        return this.To<TResultColumnBuilderImpl>();
    }
}