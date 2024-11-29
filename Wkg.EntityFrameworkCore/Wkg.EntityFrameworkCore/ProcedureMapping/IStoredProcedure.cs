using Microsoft.EntityFrameworkCore.Infrastructure;
using System.Data.Common;
using Wkg.EntityFrameworkCore.ProcedureMapping.Runtime;

namespace Wkg.EntityFrameworkCore.ProcedureMapping;

/// <summary>
/// A command object representing a stored database procedure.
/// </summary>
public interface IStoredProcedure
{
    /// <summary>
    /// The <see cref="DatabaseFacade"/> to be used for executing the stored procedure.
    /// </summary>
    public DatabaseFacade DbContext { get; set; }

    /// <summary>
    /// The stateful runtime execution context of the stored procedure. 
    /// </summary>
    /// <remarks>
    /// The execution context contains the current state of the stored procedure, such as the values of the ADO.NET <see cref="DbParameter"/>s. 
    /// Therefore it must not be shared between different threads, but can be reused for multiple sequential executions of the same stored procedure.
    /// </remarks>
    public IProcedureExecutionContext ExecutionContext { get; set; }
}
