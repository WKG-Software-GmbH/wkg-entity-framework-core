using Microsoft.EntityFrameworkCore.Infrastructure;
using System.Data.Common;
using Wkg.EntityFrameworkCore.ProcedureMapping.ResultCollections;
using Wkg.EntityFrameworkCore.ProcedureMapping.Runtime;
using Wkg.Common.Extensions;

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

/// <summary>
/// The base class for all stored procedure command objects.
/// </summary>
/// <typeparam name="TIOContainer">The type of the input/output container used to pass parameters to the stored procedure and to retrieve the result of <see langword="out"/> or <see langword="ref"/> parameters.</typeparam>
public abstract class StoredProcedure<TIOContainer> : IStoredProcedure
    where TIOContainer : class
{
    DatabaseFacade IStoredProcedure.DbContext { get; set; } = null!;

    IProcedureExecutionContext IStoredProcedure.ExecutionContext { get; set; } = null!;

    /// <summary>
    /// Uses the current execution context to invoke the stored procedure with the parameters supplied in the I/O <paramref name="container"/> instance.
    /// </summary>
    /// <param name="container">The I/O container instance holding the parameters to be passed to the stored procedure.</param>
    protected void Execute(TIOContainer container) =>
        this.To<IStoredProcedure>().ExecutionContext.Execute(this.To<IStoredProcedure>().DbContext, container);
}

/// <summary>
/// The base class for all stored procedure command objects that return one or more result rows.
/// </summary>
/// <typeparam name="TIOContainer">The type of the input/output container used to pass parameters to the stored procedure and to retrieve the result of <see langword="out"/> or <see langword="ref"/> parameters.</typeparam>
/// <typeparam name="TResult">The type of the result entities mapped to the rows returned by the procedure.</typeparam>
public abstract class StoredProcedure<TIOContainer, TResult> : StoredProcedure<TIOContainer>
    where TIOContainer : class
    where TResult : class
{
    /// <summary>
    /// Uses the current execution context to invoke the stored procedure with the parameters supplied in the I/O <paramref name="container"/> instance.
    /// </summary>
    /// <param name="container">The I/O container instance holding the parameters to be passed to the stored procedure.</param>
    /// <returns>A <see cref="IResultContainer{TResult}"/> instance containing the result of the stored procedure execution.</returns>
    protected new IResultContainer<TResult> Execute(TIOContainer container) => 
        this.To<IStoredProcedure>().ExecutionContext.Execute<TResult>(this.To<IStoredProcedure>().DbContext, container);
}