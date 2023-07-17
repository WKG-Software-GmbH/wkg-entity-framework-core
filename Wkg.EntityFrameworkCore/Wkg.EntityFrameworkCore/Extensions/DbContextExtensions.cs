using Microsoft.EntityFrameworkCore;
using Wkg.EntityFrameworkCore.ProcedureMapping;
using Wkg.EntityFrameworkCore.ProcedureMapping.Runtime;

namespace Wkg.EntityFrameworkCore.Extensions;

/// <summary>
/// Provides extension methods for <see cref="DbContext"/>.
/// </summary>
public static class DbContextExtensions
{
    /// <summary>
    /// Instantiates the previously configured stored procedure with the specified type <typeparamref name="T"/> and the provided <see cref="DbContext"/>.
    /// </summary>
    /// <typeparam name="T">The type of the stored procedure.</typeparam>
    /// <param name="context">The database context to be used.</param>
    /// <returns>The stored procedure instance.</returns>
    public static T Procedure<T>(this DbContext context) where T : IStoredProcedure, new() => 
        ProcedureRegistry.GetProcedure<T>(context.Database);
}

