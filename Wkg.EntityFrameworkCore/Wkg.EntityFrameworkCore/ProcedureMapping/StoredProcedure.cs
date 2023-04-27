using Microsoft.EntityFrameworkCore.Infrastructure;
using Wkg.EntityFrameworkCore.ProcedureMapping.ResultCollections;
using Wkg.EntityFrameworkCore.ProcedureMapping.Runtime;
using Wkg.Extensions.Common;

namespace Wkg.EntityFrameworkCore.ProcedureMapping;

public interface IStoredProcedure
{
    public DatabaseFacade DbContext { get; set; }

    public IProcedureExecutionContext ExecutionContext { get; set; }
}

public abstract class StoredProcedure<TIOContainer> : IStoredProcedure
    where TIOContainer : class
{
    DatabaseFacade IStoredProcedure.DbContext { get; set; } = null!;

    IProcedureExecutionContext IStoredProcedure.ExecutionContext { get; set; } = null!;

    protected void Execute(TIOContainer parameters) =>
        this.To<IStoredProcedure>().ExecutionContext.Execute(this.To<IStoredProcedure>().DbContext, parameters);
}

public abstract class StoredProcedure<TIOContainer, TResult> : StoredProcedure<TIOContainer>
    where TIOContainer : class
    where TResult : class
{
    protected new IResultContainer<TResult> Execute(TIOContainer parameters) => 
        this.To<IStoredProcedure>().ExecutionContext.Execute<TResult>(this.To<IStoredProcedure>().DbContext, parameters);
}