using Microsoft.EntityFrameworkCore.Infrastructure;
using System.Collections.Frozen;
using Wkg.EntityFrameworkCore.ProcedureMapping.Compiler.Output;

namespace Wkg.EntityFrameworkCore.ProcedureMapping.Runtime;

internal static class ProcedureRegistry
{
    public static FrozenDictionary<Type, ICompiledProcedure> Procedures { get; private set; } = FrozenDictionary<Type, ICompiledProcedure>.Empty;

    public static T GetProcedure<T>(DatabaseFacade database) where T : IStoredProcedure, new()
    {
        if (Procedures.TryGetValue(typeof(T), out ICompiledProcedure? compiledProcedure))
        {
            T instance = new()
            {
                ExecutionContext = compiledProcedure.CreateExecutionContext(),
                DbContext = database
            };
            return instance;
        }
        
        throw new InvalidOperationException($"Procedure {typeof(T).Name} has not been mapped or built.");
    }

    // we expect Procedures to be read more often than being written to
    // so we use a frozen dictionary to optimize for reads with a copy-on-write strategy for writes
    // this trades off startup performance for runtime performance
    internal static bool TryAddProcedure(ICompiledProcedure compiledProcedure)
    {
        Dictionary<Type, ICompiledProcedure>? procedures = new(Procedures);
        if (!procedures.TryAdd(compiledProcedure.ProcedureType, compiledProcedure))
        {
            return false;
        }
        Procedures = procedures.ToFrozenDictionary();
        return true;
    }
}