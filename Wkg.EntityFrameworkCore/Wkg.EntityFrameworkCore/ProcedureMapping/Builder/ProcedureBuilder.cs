using Microsoft.EntityFrameworkCore;
using System.Data;
using System.Data.Common;
using System.Diagnostics.CodeAnalysis;
using Wkg.EntityFrameworkCore.ProcedureMapping.Builder.ResultBinding;
using Wkg.EntityFrameworkCore.ProcedureMapping.Builder.ThrowHelpers;
using Wkg.EntityFrameworkCore.ProcedureMapping.Compiler;
using Wkg.EntityFrameworkCore.ProcedureMapping.Compiler.Output;
using Wkg.Extensions.Common;
using Wkg.Extensions.Reflection;
using Wkg.Reflection;

namespace Wkg.EntityFrameworkCore.ProcedureMapping.Builder;

public interface IProcedureBuilder
{
    string? ProcedureName { get; }
    bool IsFunction { get; }
}

public interface IProcedureBuilder<TCompiledParameter, TDataReader> : IProcedureBuilder 
    where TCompiledParameter : struct, ICompiledParameter
    where TDataReader : DbDataReader
{
    internal IProcedureCompiler<TCompiledParameter> Build();
    
    internal IReadOnlyCollection<IParameterBuilder<TCompiledParameter>> ParameterBuilders { get; }

    internal IResultBuilder<TDataReader>? ResultBuilder { get; }
}

public abstract class ProcedureBuilder<TProcedure, TIOContainer, TCompiledParameter, TDataReader, TProcedureBuilderImpl> : IProcedureBuilder<TCompiledParameter, TDataReader>
    where TProcedure : class, IStoredProcedure
    where TIOContainer : class
    where TCompiledParameter : struct, ICompiledParameter
    where TDataReader : DbDataReader
    where TProcedureBuilderImpl : ProcedureBuilder<TProcedure, TIOContainer, TCompiledParameter, TDataReader, TProcedureBuilderImpl>
{
    protected ProcedureBuilder()
    {
        if (this is not TProcedureBuilderImpl)
        {
            ThrowHelper.Throw<InvalidOperationException>($"Expected {GetType().Name} as generic type argument {nameof(TProcedureBuilderImpl)} in {GetType().Name} but got {typeof(TProcedureBuilderImpl).Name}.");
        }
    }

    protected IProcedureThrowHelper ThrowHelper { get; } = new ProcedureBuilderThrowHelper<TProcedure>();

    protected string? ProcedureName { get; private set; }

    string? IProcedureBuilder.ProcedureName => ProcedureName;

    protected bool IsFunctionValue { get; private set; } = false;

    bool IProcedureBuilder.IsFunction => IsFunctionValue;

    protected List<IParameterBuilder<TCompiledParameter>> ParameterBuilders { get; } = new();

    IReadOnlyCollection<IParameterBuilder<TCompiledParameter>> IProcedureBuilder<TCompiledParameter, TDataReader>.ParameterBuilders => ParameterBuilders;

    protected IResultBuilder<TDataReader>? ResultBuilder { get; set; }

    IResultBuilder<TDataReader>? IProcedureBuilder<TCompiledParameter, TDataReader>.ResultBuilder => ResultBuilder;

    public TProcedureBuilderImpl ToDatabaseProcedure(string name)
    {
        if (ProcedureName is not null)
        {
            ThrowHelper.Throw<InvalidOperationException>($"Attempted to set multiple values for procedure name.");
        }
        ProcedureName = name;
        return this.To<TProcedureBuilderImpl>();
    }

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

    public TProcedureBuilderImpl IsFunction(bool isFunction = true)
    {
        IsFunctionValue = isFunction;
        return this.To<TProcedureBuilderImpl>();
    }

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

    protected abstract IProcedureCompiler<TCompiledParameter> Build();

    IProcedureCompiler<TCompiledParameter> IProcedureBuilder<TCompiledParameter, TDataReader>.Build()
    {
        AssertIsValid();
        return Build();
    }
}