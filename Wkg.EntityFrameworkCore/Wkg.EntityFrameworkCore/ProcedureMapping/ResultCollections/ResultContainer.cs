namespace Wkg.EntityFrameworkCore.ProcedureMapping.ResultCollections;

/// <summary>
/// Represents a container for the result of a stored procedure.
/// </summary>
/// <typeparam name="TResult">The CLR type of the result entity representing a single row of the result set.</typeparam>
public interface IResultContainer<TResult> where TResult : class
{
    /// <summary>
    /// The number of results in this container.
    /// </summary>
    int Count { get; }

    /// <summary>
    /// Returns all result entities returned by the stored procedure.
    /// </summary>
    IReadOnlyList<TResult> AsCollection();

    /// <summary>
    /// Returns the result entity representing the first row returned by the procedure or <see langword="null"/> if the procedure did not return any rows.
    /// </summary>
    TResult? AsSingle();
}

/// <summary>
/// Represents a collection of result entites returned by a stored procedure.
/// </summary>
/// <typeparam name="TResult">The CLR type of the result entity representing a single row of the result set.</typeparam>
/// <remarks>
/// Creates a new <see cref="ResultCollection{TResult}"/> instance.
/// </remarks>
/// <param name="results">The result entities returned by the stored procedure.</param>
public readonly struct ResultCollection<TResult>(IReadOnlyList<TResult> results) : IResultContainer<TResult> where TResult : class
{
    private readonly IReadOnlyList<TResult> _results = results;

    /// <inheritdoc/>
    public int Count => _results.Count;

    /// <inheritdoc/>
    public IReadOnlyList<TResult> AsCollection() => _results;

    /// <inheritdoc/>
    public TResult? AsSingle()
    {
        if (_results.Count > 0)
        {
            return _results[0];
        }
        return null;
    }
}

/// <summary>
/// Represents a single result entity returned by a stored procedure.
/// </summary>
/// <typeparam name="TResult">The CLR type of the result entity representing a single row of the result set.</typeparam>
/// <remarks>
/// Creates a new <see cref="ResultElement{TResult}"/> instance.
/// </remarks>
/// <param name="result">The result entity returned by the stored procedure.</param>
public readonly struct ResultElement<TResult>(TResult? result) : IResultContainer<TResult> where TResult : class
{
    private readonly TResult? _result = result;

    /// <inheritdoc/>
    public int Count => _result is null ? 0 : 1;

    /// <inheritdoc/>
    public IReadOnlyList<TResult> AsCollection()
    {
        if (_result is not null)
        {
            return new[] { _result };
        }
        return Array.Empty<TResult>();
    }

    /// <inheritdoc/>
    public TResult? AsSingle() => _result;
}
