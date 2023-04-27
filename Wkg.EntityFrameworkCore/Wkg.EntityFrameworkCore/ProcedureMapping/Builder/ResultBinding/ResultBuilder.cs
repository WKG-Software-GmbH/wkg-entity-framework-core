using System.Data.Common;
using Wkg.EntityFrameworkCore.ProcedureMapping.Builder.ThrowHelpers;
using Wkg.EntityFrameworkCore.ProcedureMapping.Compiler.Output;
using Wkg.EntityFrameworkCore.ProcedureMapping.Compiler.ResultBinding;
using Wkg.Extensions.Common;

namespace Wkg.EntityFrameworkCore.ProcedureMapping.Builder.ResultBinding;

public interface IResultBuilder
{
    bool IsCollection { get; }

    internal Type ResultClrType { get; }

    IThrowHelper ThrowHelper { get; }
}

public interface IResultBuilder<TDataReader> : IResultBuilder where TDataReader : DbDataReader
{
    internal IReadOnlyCollection<IResultColumnBuilder> ColumnBuilders { get; }

    internal IResultCompiler<TDataReader> Build();
}

public abstract class ResultBuilder<TResult, TDataReader, TResultBuilderImpl> : IResultBuilder<TDataReader>
    where TResult : class
    where TDataReader : DbDataReader
    where TResultBuilderImpl : ResultBuilder<TResult, TDataReader, TResultBuilderImpl>
{
    protected IResultThrowHelper ThrowHelper { get; }

    IThrowHelper IResultBuilder.ThrowHelper => ThrowHelper;

    protected bool IsCollection { get; private set; }

    bool IResultBuilder.IsCollection => IsCollection;

    protected List<IResultColumnBuilder> ColumnBuilders { get; } = new();

    IReadOnlyCollection<IResultColumnBuilder> IResultBuilder<TDataReader>.ColumnBuilders => ColumnBuilders;

    protected Type ResultClrType { get; }

    Type IResultBuilder.ResultClrType => ResultClrType;

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

    public TResultBuilderImpl AsCollection()
    {
        IsCollection = true;
        return this.To<TResultBuilderImpl>();
    }

    public TResultBuilderImpl AsSingle()
    {
        IsCollection = false;
        return this.To<TResultBuilderImpl>();
    }

    protected abstract IResultCompiler<TDataReader> Build();

    IResultCompiler<TDataReader> IResultBuilder<TDataReader>.Build() => Build();
}
