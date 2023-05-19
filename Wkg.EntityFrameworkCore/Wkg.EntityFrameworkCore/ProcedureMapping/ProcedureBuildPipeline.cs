using System.Data.Common;
using Wkg.EntityFrameworkCore.ProcedureMapping.Builder;
using Wkg.EntityFrameworkCore.ProcedureMapping.Compiler.Output;
using Wkg.EntityFrameworkCore.ProcedureMapping.Runtime;

namespace Wkg.EntityFrameworkCore.ProcedureMapping;

/// <summary>
/// Provides the base class for all procedure build pipelines.
/// </summary>
public abstract class ProcedureBuildPipeline
{
    /// <summary>
    /// Executes the procedure build pipeline.
    /// </summary>
    /// <typeparam name="TCompiledParameter">The concrete type of the compiled parameters.</typeparam>
    /// <typeparam name="TDataReader">The concrete type of the <see cref="DbDataReader"/> to be used to read the result set.</typeparam>
    /// <param name="procedureBuilder">The procedure builder to build.</param>
    protected static void Execute<TCompiledParameter, TDataReader>(IProcedureBuilder<TCompiledParameter, TDataReader> procedureBuilder) 
        where TCompiledParameter : struct, ICompiledParameter
        where TDataReader : DbDataReader
    {
        ICompiledProcedure compiledProcedure = procedureBuilder
            .Build()
            .Compile(
                procedureBuilder.ParameterBuilders
                    .Select(p => p
                        .Build()
                        .Compile())
                    .ToArray(), 
                procedureBuilder.ResultBuilder?
                    .Build()
                    .Compile(procedureBuilder.ResultBuilder.ColumnBuilders
                        .Select(c => c
                            .Build()
                            .Compile())
                        .ToArray()));

        ProcedureCache.RegisteredProcedures.TryAdd(compiledProcedure.ProcedureType, compiledProcedure);
    }
}
