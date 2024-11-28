using Microsoft.EntityFrameworkCore.Infrastructure;
using Wkg.EntityFrameworkCore.ProcedureMapping.ResultCollections;
using Wkg.EntityFrameworkCore.ProcedureMapping.Runtime;

namespace Wkg.EntityFrameworkCore.ProcedureMapping;

/// <summary>
/// The base class for all stored procedure command objects.
/// </summary>
public abstract class StoredProcedure : IStoredProcedure
{
    DatabaseFacade IStoredProcedure.DbContext { get; set; } = null!;

    IProcedureExecutionContext IStoredProcedure.ExecutionContext { get; set; } = null!;
}

/// <summary>
/// The base class for all stored procedure command objects.
/// </summary>
/// <typeparam name="TIOContainer">The type of the input/output container used to pass parameters to the stored procedure and to retrieve the result of <see langword="out"/> or <see langword="ref"/> parameters.</typeparam>
public abstract class StoredProcedure<TIOContainer> : StoredProcedure
    where TIOContainer : class
{
    /// <summary>
    /// Uses the current execution context to invoke the stored procedure with the parameters supplied in the I/O <paramref name="container"/> instance.
    /// </summary>
    /// <param name="container">The I/O container instance holding the parameters to be passed to the stored procedure.</param>
    protected void Execute(TIOContainer container)
    {
        IStoredProcedure self = this;
        self.ExecutionContext.Execute(self.DbContext, container);
    }

    /// <summary>
    /// Uses the current execution context to asynchronously invoke the stored procedure with the parameters supplied in the I/O <paramref name="container"/> instance.
    /// </summary>
    /// <param name="container">The I/O container instance holding the parameters to be passed to the stored procedure.</param>
    /// <param name="cancellationToken">The cancellation token to use for asynchronous operations.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    protected Task ExecuteAsync(TIOContainer container, CancellationToken cancellationToken = default)
    {
        IStoredProcedure self = this;
        return self.ExecutionContext.ExecuteAsync(self.DbContext, container, cancellationToken);
    }
}

/// <summary>
/// The base class for all stored procedure command objects that return one or more result rows.
/// </summary>
/// <typeparam name="TIOContainer">The type of the input/output container used to pass parameters to the stored procedure and to retrieve the result of <see langword="out"/> or <see langword="ref"/> parameters.</typeparam>
/// <typeparam name="TResult">The type of the result entities mapped to the rows returned by the procedure.</typeparam>
public abstract class StoredProcedure<TIOContainer, TResult> : StoredProcedure
    where TIOContainer : class
    where TResult : class
{
    /// <summary>
    /// Uses the current execution context to invoke the stored procedure with the parameters supplied in the I/O <paramref name="container"/> instance.
    /// </summary>
    /// <param name="container">The I/O container instance holding the parameters to be passed to the stored procedure.</param>
    /// <returns>A <see cref="IResultContainer{TResult}"/> instance containing the result of the stored procedure execution.</returns>
    protected IResultContainer<TResult> Execute(TIOContainer container)
    {
        IStoredProcedure self = this;
        return self.ExecutionContext.Execute<TResult>(self.DbContext, container);
    }

    /// <summary>
    /// Uses the current execution context to asynchronously invoke the stored procedure with the parameters supplied in the I/O <paramref name="container"/> instance.
    /// </summary>
    /// <param name="container">The I/O container instance holding the parameters to be passed to the stored procedure.</param>
    /// <param name="cancellationToken">The cancellation token to use for asynchronous operations.</param>
    /// <returns>A task that represents the asynchronous operation and contains a <see cref="IResultContainer{TResult}"/> instance containing the result of the stored procedure execution.</returns>
    protected Task<IResultContainer<TResult>> ExecuteAsync(TIOContainer container, CancellationToken cancellationToken = default)
    {
        IStoredProcedure self = this;
        return self.ExecutionContext.ExecuteAsync<TResult>(self.DbContext, container, cancellationToken);
    }
}