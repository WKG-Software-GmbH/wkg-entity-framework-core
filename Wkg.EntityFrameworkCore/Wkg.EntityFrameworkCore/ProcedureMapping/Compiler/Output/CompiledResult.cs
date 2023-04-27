using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Wkg.EntityFrameworkCore.ProcedureMapping.Compiler.Output;

public abstract class CompiledResult
{
    internal bool IsCollection { get; }

    protected CompiledResult(bool isCollection) => IsCollection = isCollection;

    public abstract object ReadFrom(DbDataReader reader);
}

public abstract class CompiledResult<TDataReader> : CompiledResult
    where TDataReader : DbDataReader
{
    protected CompiledResultFactory<TDataReader> CompiledResultFactory { get; }

    protected CompiledResult(bool isCollection, CompiledResultFactory<TDataReader> compiledResultFactory) : base(isCollection)
    {
        CompiledResultFactory = compiledResultFactory;
    }
}
