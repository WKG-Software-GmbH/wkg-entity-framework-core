﻿using System.Collections.Immutable;
using Wkg.EntityFrameworkCore.ProcedureMapping.Runtime;

namespace Wkg.EntityFrameworkCore.ProcedureMapping.Compiler.Output;

/// <summary>
/// A stateless representation of a compiled stored procedure that can be used to create a stateful <see cref="IProcedureExecutionContext"/>.
/// </summary>
public interface ICompiledProcedure
{
    /// <summary>
    /// The CLR type of the command object managing this procedure.
    /// </summary>
    internal Type ProcedureType { get; }

    /// <summary>
    /// Creates a new <see cref="IProcedureExecutionContext"/> for this procedure.
    /// </summary>
    IProcedureExecutionContext CreateExecutionContext();
}

/// <summary>
/// Represents a stateless compiled stored procedure that can be used to create a stateful <see cref="IProcedureExecutionContext"/>.
/// </summary>
/// <typeparam name="TCompiledParameter">The concrete type of the compiled parameters.</typeparam>
/// <remarks>
/// Creates a new <see cref="CompiledProcedure{TCompiledParameter}"/> instance.
/// </remarks>
/// <param name="procedureName">The name of the stored procedure.</param>
/// <param name="isFunction">Indicates whether the stored procedure is a database function.</param>
/// <param name="parameters">The compiled parameters of this stored procedure.</param>
/// <param name="procedureType">The CLR type of the command object managing this procedure.</param>
/// <param name="compiledResult">The compiled result returned by this stored procedure.</param>
public sealed class CompiledProcedure<TCompiledParameter>(string procedureName, bool isFunction, TCompiledParameter[] parameters, Type procedureType, CompiledResult? compiledResult) 
    : ICompiledProcedure where TCompiledParameter : struct, ICompiledParameter
{
    /// <summary>
    /// The name of the stored procedure.
    /// </summary>
    internal string ProcedureName { get; } = procedureName;

    /// <summary>
    /// Indicates whether the stored procedure is a database function.
    /// </summary>
    internal bool IsFunction { get; } = isFunction;

    /// <summary>
    /// The compiled parameters of this stored procedure.
    /// </summary>
    internal ImmutableArray<TCompiledParameter> CompiledParameters { get; } = [.. parameters];

    /// <summary>
    /// The number of parameters of this stored procedure.
    /// </summary>
    internal int ParameterCount => CompiledParameters.Length;

    /// <summary>
    /// The compiled result returned by this stored procedure.
    /// </summary>
    internal CompiledResult? CompiledResult { get; } = compiledResult;

    Type ICompiledProcedure.ProcedureType => _procedureType;

    private readonly Type _procedureType = procedureType;

    /// <summary>
    /// Creates a new <see cref="IProcedureExecutionContext"/> for this procedure.
    /// </summary>
    /// <remarks>
    /// Execution contexts are stateful and should never be shared between threads.
    /// </remarks>
    public IProcedureExecutionContext CreateExecutionContext() => new ProcedureExecutionContext<TCompiledParameter>(this);
}
