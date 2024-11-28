using Microsoft.EntityFrameworkCore.Infrastructure;
using System.Data.Common;
using Wkg.EntityFrameworkCore.ProcedureMapping.ResultCollections;

namespace Wkg.EntityFrameworkCore.ProcedureMapping.Runtime;

/// <summary>
/// Represents the stateful runtime execution context of a stored procedure. 
/// </summary>
/// <remarks>
/// This class is responsible for handling, populating, and caching the ADO.NET <see cref="DbParameter"/>s, creating and executing the ADO.NET <see cref="DbCommand"/> and storing potential results into a <see cref="IResultContainer{TResult}"/> object.
/// </remarks>
public interface IProcedureExecutionContext
{
    /// <summary>
    /// Executes the stored procedure against the provided <see cref="DatabaseFacade"/> using the provided I/O <paramref name="container"/> object to load input parameters and store output parameters.
    /// </summary>
    /// <param name="dbContext">The <see cref="DatabaseFacade"/> to be used for the ADO.NET call.</param>
    /// <param name="container">The I/O container object.</param>
    void Execute(DatabaseFacade dbContext, object container);

    /// <summary>
    /// Asynchronously executes the stored procedure against the provided <see cref="DatabaseFacade"/> using the provided I/O <paramref name="container"/> object to load input parameters and store output parameters.
    /// </summary>
    /// <param name="dbContext">The <see cref="DatabaseFacade"/> to be used for the ADO.NET call.</param>
    /// <param name="container">The I/O container object.</param>
    /// <param name="cancellationToken">The token to monitor for cancellation requests.</param>
    Task ExecuteAsync(DatabaseFacade dbContext, object container, CancellationToken cancellationToken = default);

    /// <summary>
    /// Executes the stored procedure against the provided <see cref="DatabaseFacade"/> using the provided I/O <paramref name="container"/> object to load input parameters and store output parameters. Results are returned in a <see cref="IResultContainer{TResult}"/> object.
    /// </summary>
    /// <typeparam name="TResult">The type of the result entities constructed from the result rows.</typeparam>
    /// <param name="dbContext">The <see cref="DatabaseFacade"/> to be used for the ADO.NET call.</param>
    /// <param name="container">The I/O container object.</param>
    /// <returns>The result container holding the result entities.</returns>
    IResultContainer<TResult> Execute<TResult>(DatabaseFacade dbContext, object container) where TResult : class;

    /// <summary>
    /// Asynchronously executes the stored procedure against the provided <see cref="DatabaseFacade"/> using the provided I/O <paramref name="container"/> object to load input parameters and store output parameters. Results are returned in a <see cref="IResultContainer{TResult}"/> object.
    /// </summary>
    /// <typeparam name="TResult">The type of the result entities constructed from the result rows.</typeparam>
    /// <param name="dbContext">The <see cref="DatabaseFacade"/> to be used for the ADO.NET call.</param>
    /// <param name="container">The I/O container object.</param>
    /// <param name="cancellationToken">The token to monitor for cancellation requests.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the result container holding the result entities.</returns>
    Task<IResultContainer<TResult>> ExecuteAsync<TResult>(DatabaseFacade dbContext, object container, CancellationToken cancellationToken = default) where TResult : class;
}
