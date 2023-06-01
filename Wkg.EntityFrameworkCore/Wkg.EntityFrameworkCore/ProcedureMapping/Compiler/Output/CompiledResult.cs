using System.Data.Common;

namespace Wkg.EntityFrameworkCore.ProcedureMapping.Compiler.Output;

/// <summary>
/// The base class for a stateless compiled result returned by a stored procedure.
/// </summary>
public abstract class CompiledResult
{
    /// <summary>
    /// Indicates whether the result is a collection or a single entity.
    /// </summary>
    internal bool IsCollection { get; }

    /// <summary>
    /// Creates a new <see cref="CompiledResult"/> instance.
    /// </summary>
    /// <param name="isCollection">Indicates whether the result is a collection or a single entity.</param>
    protected CompiledResult(bool isCollection) => IsCollection = isCollection;

    /// <summary>
    /// Constructs a single result entity from the next row of the provided <see cref="DbDataReader"/>.
    /// </summary>
    public abstract object ReadFrom(DbDataReader reader);
}

/// <summary>
/// The base class for a stateless compiled result returned by a stored procedure using the specified <typeparamref name="TDataReader"/>.
/// </summary>
/// <typeparam name="TDataReader">The type of the <see cref="DbDataReader"/> used to read the result.</typeparam>
public abstract class CompiledResult<TDataReader> : CompiledResult
    where TDataReader : DbDataReader
{
    /// <summary>
    /// The factory used to construct a result entity from a row of specified <typeparamref name="TDataReader"/>.
    /// </summary>
    protected CompiledResultFactory<TDataReader> CompiledResultFactory { get; }

    /// <summary>
    /// Creates a new <see cref="CompiledResult{TDataReader}"/> instance.
    /// </summary>
    /// <param name="isCollection">Indicates whether the result is a collection or a single entity.</param>
    /// <param name="compiledResultFactory">The factory used to construct a result entity from a row of specified <typeparamref name="TDataReader"/>.</param>
    protected CompiledResult(bool isCollection, CompiledResultFactory<TDataReader> compiledResultFactory) : base(isCollection)
    {
        CompiledResultFactory = compiledResultFactory;
    }
}
