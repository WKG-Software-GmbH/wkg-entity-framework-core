using Microsoft.EntityFrameworkCore.Infrastructure;
using Wkg.EntityFrameworkCore.ProcedureMapping.Compiler.Output;

namespace Wkg.EntityFrameworkCore.ProcedureMapping.Runtime;

internal static class ProcedureCache
{
    public static Dictionary<Type, ICompiledProcedure> RegisteredProcedures { get; } = new();

    public static T GetProcedure<T>(DatabaseFacade database) where T : IStoredProcedure, new()
    {
        if (RegisteredProcedures.TryGetValue(typeof(T), out ICompiledProcedure? compiledProcedure))
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
}