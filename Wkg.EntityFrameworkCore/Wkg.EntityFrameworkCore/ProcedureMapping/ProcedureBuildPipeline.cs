using System.Data.Common;
using Wkg.EntityFrameworkCore.ProcedureMapping.Builder;
using Wkg.EntityFrameworkCore.ProcedureMapping.Compiler.Output;
using Wkg.EntityFrameworkCore.ProcedureMapping.Runtime;

namespace Wkg.EntityFrameworkCore.ProcedureMapping;

public abstract class ProcedureBuildPipeline
{
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
