using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage;
using System.Collections.Immutable;
using System.Data;
using System.Data.Common;
using System.Runtime.CompilerServices;
using Wkg.EntityFrameworkCore.ProcedureMapping.Compiler.Output;
using Wkg.EntityFrameworkCore.ProcedureMapping.ResultCollections;

namespace Wkg.EntityFrameworkCore.ProcedureMapping.Runtime;

internal sealed class ProcedureExecutionContext<TCompiledParameter> : IProcedureExecutionContext where TCompiledParameter : struct, ICompiledParameter
{
    private CompiledProcedure<TCompiledParameter> CompiledProcedure { get; }
    private DbParameter?[] DbParameters { get; }

    public ProcedureExecutionContext(CompiledProcedure<TCompiledParameter> compiledProcedure)
    {
        CompiledProcedure = compiledProcedure;

        // allocate local array for DbParameters for ADO.NET call
        DbParameters = CompiledProcedure.ParameterCount > 0
            ? new DbParameter?[CompiledProcedure.ParameterCount]
            : [];
    }

    /// <summary>
    /// Prepares the ADO.NET <see cref="DbParameter"/> array by loading the values from the I/O container <paramref name="context"/> object using the generated get accessors in the compiled parameters.
    /// </summary>
    /// <param name="context">The I/O container object.</param>
    /// <param name="compiledParameters">The compiled parameters.</param>
    private void BeforeExecution(object context, ImmutableArray<TCompiledParameter> compiledParameters)
    {
        for (int i = 0; i < compiledParameters.Length; i++)
        {
            ref DbParameter? param = ref DbParameters[i];
            compiledParameters[i].Load(ref param, context);
        }
    }

    /// <summary>
    /// Stores the values of <see langword="ref"/> and <see langword="out"/> parameters in the ADO.NET <see cref="DbParameter"/> array back into the I/O container <paramref name="context"/> object using the generated set accessors in the compiled parameters.
    /// </summary>
    /// <param name="context">The I/O container object.</param>
    /// <param name="compiledParameters">The compiled parameters.</param>
    private void AfterExecution(object context, ImmutableArray<TCompiledParameter> compiledParameters)
    {
        for (int i = 0; i < compiledParameters.Length; i++)
        {
            ref DbParameter parameter = ref DbParameters[i]!;
            TCompiledParameter compiledParameter = compiledParameters[i];
            if (compiledParameter.IsOutput)
            {
                compiledParameter.Store(ref parameter, context);
            }
            if (parameter is IDisposable disposable)
            {
                disposable.Dispose();
                parameter = null!;
            }
        }
    }

    /// <summary>
    /// Stores the values of <see langword="ref"/> and <see langword="out"/> parameters in the ADO.NET <see cref="DbParameter"/> array, as well as scalar return values of procedures, back into the I/O container <paramref name="context"/> object using the generated set accessors in the compiled parameters.
    /// </summary>
    /// <param name="context">The I/O container object.</param>
    /// <param name="returnValue">The return value of the procedure or function.</param>
    /// <param name="compiledParameters">The compiled parameters.</param>
    private void AfterExecution(object context, object? returnValue, ImmutableArray<TCompiledParameter> compiledParameters)
    {
        for (int i = 0; i < compiledParameters.Length; i++)
        {
            ref DbParameter parameter = ref DbParameters[i]!;
            TCompiledParameter compiledParameter = compiledParameters[i];

            // update return value of procedure in return parameter
            // (return value of function is stored in the return parameter by the ADO.NET provider)
            if (!CompiledProcedure.IsFunction && parameter.Direction is ParameterDirection.ReturnValue)
            {
                parameter.Value = returnValue;
            }
            if (compiledParameter.IsOutput)
            {
                // update ref/out/return parameters in context
                compiledParameter.Store(ref parameter, context);
            }
            if (parameter is IDisposable disposable)
            {
                disposable.Dispose();
                parameter = null!;
            }
        }
    }

    /// <summary>
    /// Executes the stored procedure against the provided <see cref="DatabaseFacade"/> using the pre-populated ADO.NET <see cref="DbParameter"/> array.
    /// </summary>
    /// <param name="databaseFacade">The <see cref="DatabaseFacade"/> to be used for the ADO.NET call.</param>
    /// <returns>The single value returned by this procedure or function.</returns>
    private object? ExecuteProcedureScalar(DatabaseFacade databaseFacade)
    {
        DbConnection connection = databaseFacade.GetDbConnection();
        if (connection.State is ConnectionState.Closed)
        {
            connection.Open();
        }
        using DbCommand cmd = connection.CreateCommand();
        cmd.Transaction = databaseFacade.CurrentTransaction?.GetDbTransaction();
        cmd.CommandText = CompiledProcedure.ProcedureName;
        cmd.CommandType = CommandType.StoredProcedure;
        foreach (DbParameter? parameter in DbParameters)
        {
            if (CompiledProcedure.IsFunction || parameter!.Direction is not ParameterDirection.ReturnValue)
            {
                cmd.Parameters.Add(parameter!);
            }
        }
        return cmd.ExecuteScalar();
    }

    /// <summary>
    /// Asynchronously executes the stored procedure against the provided <see cref="DatabaseFacade"/> using the pre-populated ADO.NET <see cref="DbParameter"/> array.
    /// </summary>
    /// <param name="databaseFacade">The <see cref="DatabaseFacade"/> to be used for the ADO.NET call.</param>
    /// <param name="cancellationToken">The token to monitor for cancellation requests.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the single value returned by this procedure or function.</returns>
    private async Task<object?> ExecuteProcedureScalarAsync(DatabaseFacade databaseFacade, CancellationToken cancellationToken = default)
    {
        DbConnection connection = databaseFacade.GetDbConnection();
        if (connection.State is ConnectionState.Closed)
        {
            await connection.OpenAsync(cancellationToken).ConfigureAwait(false);
        }
        await using DbCommand cmd = connection.CreateCommand();
        cmd.Transaction = databaseFacade.CurrentTransaction?.GetDbTransaction();
        cmd.CommandText = CompiledProcedure.ProcedureName;
        cmd.CommandType = CommandType.StoredProcedure;
        foreach (DbParameter? parameter in DbParameters)
        {
            if (CompiledProcedure.IsFunction || parameter!.Direction is not ParameterDirection.ReturnValue)
            {
                cmd.Parameters.Add(parameter!);
            }
        }
        return await cmd.ExecuteScalarAsync(cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    /// Executes the stored procedure against the provided <see cref="DatabaseFacade"/> using the pre-populated ADO.NET <see cref="DbParameter"/> array.
    /// </summary>
    /// <typeparam name="TResult">The type of the entity to be returned.</typeparam>
    /// <param name="databaseFacade">The <see cref="DatabaseFacade"/> to be used for the ADO.NET call.</param>
    /// <returns>The first row returned by this procedure, stored in a <typeparamref name="TResult"/> entity.</returns>
    private ResultElement<TResult> ExecuteProcedureReaderSingle<TResult>(DatabaseFacade databaseFacade)
        where TResult : class
    {
        DbConnection connection = databaseFacade.GetDbConnection();
        if (connection.State is ConnectionState.Closed)
        {
            connection.Open();
        }
        using DbCommand cmd = connection.CreateCommand();
        cmd.Transaction = databaseFacade.CurrentTransaction?.GetDbTransaction();
        cmd.CommandText = CompiledProcedure.ProcedureName;
        cmd.CommandType = CommandType.StoredProcedure;
        foreach (DbParameter? parameter in DbParameters)
        {
            cmd.Parameters.Add(parameter!);
        }
        using DbDataReader reader = cmd.ExecuteReader();
        TResult? result = null;
        if (reader.Read())
        {
            object rawResult = CompiledProcedure.CompiledResult!.ReadFrom(reader);
            result = Unsafe.As<TResult>(rawResult);
        }
        return new ResultElement<TResult>(result);
    }

    /// <summary>
    /// Asynchronously executes the stored procedure against the provided <see cref="DatabaseFacade"/> using the pre-populated ADO.NET <see cref="DbParameter"/> array.
    /// </summary>
    /// <typeparam name="TResult">The type of the entity to be returned.</typeparam>
    /// <param name="databaseFacade">The <see cref="DatabaseFacade"/> to be used for the ADO.NET call.</param>
    /// <param name="cancellationToken">The token to monitor for cancellation requests.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the first row returned by this procedure, stored in a <typeparamref name="TResult"/> entity.</returns>
    private async Task<ResultElement<TResult>> ExecuteProcedureReaderSingleAsync<TResult>(DatabaseFacade databaseFacade, CancellationToken cancellationToken = default)
        where TResult : class
    {
        DbConnection connection = databaseFacade.GetDbConnection();
        if (connection.State is ConnectionState.Closed)
        {
            await connection.OpenAsync(cancellationToken).ConfigureAwait(false);
        }
        await using DbCommand cmd = connection.CreateCommand();
        cmd.Transaction = databaseFacade.CurrentTransaction?.GetDbTransaction();
        cmd.CommandText = CompiledProcedure.ProcedureName;
        cmd.CommandType = CommandType.StoredProcedure;
        foreach (DbParameter? parameter in DbParameters)
        {
            cmd.Parameters.Add(parameter!);
        }
        await using DbDataReader reader = await cmd.ExecuteReaderAsync(cancellationToken).ConfigureAwait(false);
        TResult? result = null;
        if (await reader.ReadAsync(cancellationToken).ConfigureAwait(false))
        {
            object rawResult = CompiledProcedure.CompiledResult!.ReadFrom(reader);
            result = Unsafe.As<TResult>(rawResult);
        }
        return new ResultElement<TResult>(result);
    }

    /// <summary>
    /// Executes the stored procedure against the provided <see cref="DatabaseFacade"/> using the pre-populated ADO.NET <see cref="DbParameter"/> array.
    /// </summary>
    /// <typeparam name="TResult">The type of the result returned by the stored procedure.</typeparam>
    /// <param name="databaseFacade">The <see cref="DatabaseFacade"/> to be used for the ADO.NET call.</param>
    /// <returns>A collection of <typeparamref name="TResult"/> entities representing all rows returned by this procedure.</returns>
    private ResultCollection<TResult> ExecuteProcedureReader<TResult>(DatabaseFacade databaseFacade)
        where TResult : class
    {
        DbConnection connection = databaseFacade.GetDbConnection();
        if (connection.State is ConnectionState.Closed)
        {
            connection.Open();
        }
        using DbCommand cmd = connection.CreateCommand();
        cmd.Transaction = databaseFacade.CurrentTransaction?.GetDbTransaction();
        cmd.CommandText = CompiledProcedure.ProcedureName;
        cmd.CommandType = CommandType.StoredProcedure;
        foreach (DbParameter? parameter in DbParameters)
        {
            cmd.Parameters.Add(parameter!);
        }
        using DbDataReader reader = cmd.ExecuteReader();
        List<TResult> results = [];
        while (reader.Read())
        {
            object rawResult = CompiledProcedure.CompiledResult!.ReadFrom(reader);
            TResult result = Unsafe.As<TResult>(rawResult);
            results.Add(result);
        }
        return new ResultCollection<TResult>(results);
    }

    /// <summary>
    /// Asynchronously executes the stored procedure against the provided <see cref="DatabaseFacade"/> using the pre-populated ADO.NET <see cref="DbParameter"/> array.
    /// </summary>
    /// <typeparam name="TResult">The type of the result returned by the stored procedure.</typeparam>
    /// <param name="databaseFacade">The <see cref="DatabaseFacade"/> to be used for the ADO.NET call.</param>
    /// <param name="cancellationToken">The token to monitor for cancellation requests.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains a collection of <typeparamref name="TResult"/> entities representing all rows returned by this procedure.</returns>
    private async Task<ResultCollection<TResult>> ExecuteProcedureReaderAsync<TResult>(DatabaseFacade databaseFacade, CancellationToken cancellationToken = default)
        where TResult : class
    {
        DbConnection connection = databaseFacade.GetDbConnection();
        if (connection.State is ConnectionState.Closed)
        {
            await connection.OpenAsync(cancellationToken).ConfigureAwait(false);
        }
        await using DbCommand cmd = connection.CreateCommand();
        cmd.Transaction = databaseFacade.CurrentTransaction?.GetDbTransaction();
        cmd.CommandText = CompiledProcedure.ProcedureName;
        cmd.CommandType = CommandType.StoredProcedure;
        foreach (DbParameter? parameter in DbParameters)
        {
            cmd.Parameters.Add(parameter!);
        }
        await using DbDataReader reader = await cmd.ExecuteReaderAsync(cancellationToken).ConfigureAwait(false);
        List<TResult> results = [];
        while (await reader.ReadAsync(cancellationToken).ConfigureAwait(false))
        {
            object rawResult = CompiledProcedure.CompiledResult!.ReadFrom(reader);
            TResult result = Unsafe.As<TResult>(rawResult);
            results.Add(result);
        }
        return new ResultCollection<TResult>(results);
    }

    /// <inheritdoc/>
    public void Execute(DatabaseFacade dbContext, object context)
    {
        ImmutableArray<TCompiledParameter> parameters = CompiledProcedure.CompiledParameters;
        BeforeExecution(context, parameters);
        object? returnValue = ExecuteProcedureScalar(dbContext);
        AfterExecution(context, returnValue, parameters);
    }

    public async Task ExecuteAsync(DatabaseFacade dbContext, object context, CancellationToken cancellationToken = default)
    {
        ImmutableArray<TCompiledParameter> parameters = CompiledProcedure.CompiledParameters;
        BeforeExecution(context, parameters);
        object? returnValue = await ExecuteProcedureScalarAsync(dbContext, cancellationToken).ConfigureAwait(false);
        AfterExecution(context, returnValue, parameters);
    }

    /// <inheritdoc/>
    public IResultContainer<TResult> Execute<TResult>(DatabaseFacade dbContext, object context) where TResult : class
    {
        ImmutableArray<TCompiledParameter> parameters = CompiledProcedure.CompiledParameters;
        BeforeExecution(context, parameters);
        IResultContainer<TResult> result = CompiledProcedure.CompiledResult!.IsCollection
            ? ExecuteProcedureReader<TResult>(dbContext)
            : ExecuteProcedureReaderSingle<TResult>(dbContext);
        AfterExecution(context, parameters);
        return result;
    }

    /// <inheritdoc/>
    public async Task<IResultContainer<TResult>> ExecuteAsync<TResult>(DatabaseFacade dbContext, object context, CancellationToken cancellationToken = default) where TResult : class
    {
        ImmutableArray<TCompiledParameter> parameters = CompiledProcedure.CompiledParameters;
        BeforeExecution(context, parameters);
        IResultContainer<TResult> result = CompiledProcedure.CompiledResult!.IsCollection
            ? await ExecuteProcedureReaderAsync<TResult>(dbContext, cancellationToken).ConfigureAwait(false)
            : await ExecuteProcedureReaderSingleAsync<TResult>(dbContext, cancellationToken).ConfigureAwait(false);
        AfterExecution(context, parameters);
        return result;
    }
}
