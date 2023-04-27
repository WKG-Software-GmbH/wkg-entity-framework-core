using Microsoft.EntityFrameworkCore;
using Wkg.EntityFrameworkCore.ProcedureMapping;
using Wkg.EntityFrameworkCore.ProcedureMapping.Compiler.Output;
using Wkg.EntityFrameworkCore.ProcedureMapping.Runtime;

namespace Wkg.EntityFrameworkCore.Extensions;

public static class DbContextExtensions
{
    public static T Procedure<T>(this DbContext context) where T : IStoredProcedure, new() => 
        ProcedureCache.GetProcedure<T>(context.Database);
}

