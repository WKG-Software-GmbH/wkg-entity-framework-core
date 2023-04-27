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

public interface IParameterBuilder
{
    ParameterDirection ParameterDirection { get; }

    string? ParameterName { get; }

    bool IsOutput { get; }

    int Size { get; }

    ParameterBuilderContext Context { get; }
}

public interface IParameterBuilder<out TCompiledParameter> : IParameterBuilder where TCompiledParameter : struct, ICompiledParameter
{
    internal IParameterCompiler<TCompiledParameter> Build();
}

public abstract class ParameterBuilder<TIOContainer, TParameter, TCompiledParameter, TParameterBuilderImpl> : IParameterBuilder<TCompiledParameter>
    where TIOContainer : class
    where TCompiledParameter : struct, ICompiledParameter
{
    protected string ParameterName { get; set; }

    protected int Size { get; set; }

    protected ParameterDirection ParameterDirection { get; private set; } = ParameterDirection.Input;

    bool IParameterBuilder.IsOutput => ParameterDirection is ParameterDirection.Output or ParameterDirection.InputOutput or ParameterDirection.ReturnValue;

    ParameterDirection IParameterBuilder.ParameterDirection => ParameterDirection;

    int IParameterBuilder.Size => Size;

    string? IParameterBuilder.ParameterName => ParameterName;

    ParameterBuilderContext IParameterBuilder.Context => Context;

    protected ParameterBuilderContext Context { get; }

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

    public TParameterBuilderImpl HasName(string parameterName)
    {
        ParameterName = parameterName;
        return this.To<TParameterBuilderImpl>();
    }

    public TParameterBuilderImpl HasDirection(ParameterDirection direction)
    {
        ParameterDirection = direction;
        if (direction is ParameterDirection.ReturnValue && ParameterName is null)
        {
            ParameterName = $"ReturnValue_{Guid.NewGuid():N}";
        }
        return this.To<TParameterBuilderImpl>();
    }

    public TParameterBuilderImpl HasSize(int size)
    {
        Size = size;
        return this.To<TParameterBuilderImpl>();
    }

    protected abstract IParameterCompiler<TCompiledParameter> Build();

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
