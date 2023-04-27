using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using System.Data.Common;
using System.Data;
using Wkg.EntityFrameworkCore.ProcedureMapping.Compiler.Output;
using Wkg.EntityFrameworkCore.ProcedureMapping.ResultCollections;
using System.Runtime.CompilerServices;
using Microsoft.EntityFrameworkCore.Storage;

namespace Wkg.EntityFrameworkCore.ProcedureMapping.Runtime;

public interface IProcedureExecutionContext
{
    void Execute(DatabaseFacade dbContext, object context);

    IResultContainer<TResult> Execute<TResult>(DatabaseFacade dbContext, object context) where TResult : class;
}

internal sealed class ProcedureExecutionContext<TCompiledParameter> : IProcedureExecutionContext where TCompiledParameter : struct, ICompiledParameter
{
    protected CompiledProcedure<TCompiledParameter> CompiledProcedure { get; }
    protected DbParameter?[] DbParameters { get; }

    public ProcedureExecutionContext(CompiledProcedure<TCompiledParameter> compiledProcedure)
    {
        CompiledProcedure = compiledProcedure;

        // allocate local array for DbParameters for ADO.NET call
        DbParameters = CompiledProcedure.ParameterCount > 0
            ? new DbParameter?[CompiledProcedure.ParameterCount]
            : Array.Empty<DbParameter>();
    }

    /// <summary>
    /// Prepares the ADO.NET <see cref="DbParameter"/> array by loading the values from the I/O container <paramref name="context"/> object using the generated get accessors in the compiled parameters.
    /// </summary>
    /// <param name="context">The I/O container object.</param>
    /// <param name="compiledParameters">The compiled parameters.</param>
    private void BeforeExecution(object context, ref ReadOnlySpan<TCompiledParameter> compiledParameters)
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
    private void AfterExecution(object context, ref ReadOnlySpan<TCompiledParameter> compiledParameters)
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
    private void AfterExecution(object context, object? returnValue, ref ReadOnlySpan<TCompiledParameter> compiledParameters)
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

    private IResultContainer<TResult> ExecuteProcedureReaderSingle<TResult>(DatabaseFacade databaseFacade)
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

    private IResultContainer<TResult> ExecuteProcedureReader<TResult>(DatabaseFacade databaseFacade)
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
        List<TResult> results = new();
        while (reader.Read())
        {
            object rawResult = CompiledProcedure.CompiledResult!.ReadFrom(reader);
            TResult result = Unsafe.As<TResult>(rawResult);
            results.Add(result!);
        }
        return new ResultCollection<TResult>(results);
    }

    public void Execute(DatabaseFacade dbContext, object context)
    {
        ReadOnlySpan<TCompiledParameter> parameters = CompiledProcedure.CompiledParameters;
        BeforeExecution(context, ref parameters);
        object? returnValue = ExecuteProcedureScalar(dbContext);
        AfterExecution(context, returnValue, ref parameters);
    }

    public IResultContainer<TResult> Execute<TResult>(DatabaseFacade dbContext, object context) where TResult : class
    {
        ReadOnlySpan<TCompiledParameter> parameters = CompiledProcedure.CompiledParameters;
        BeforeExecution(context, ref parameters);
        IResultContainer<TResult> result = CompiledProcedure.CompiledResult!.IsCollection
            ? ExecuteProcedureReader<TResult>(dbContext)
            : ExecuteProcedureReaderSingle<TResult>(dbContext);
        AfterExecution(context, ref parameters);
        return result;
    }
}
