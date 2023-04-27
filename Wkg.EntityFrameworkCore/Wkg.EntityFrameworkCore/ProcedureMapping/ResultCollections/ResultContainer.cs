using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Wkg.EntityFrameworkCore.ProcedureMapping.ResultCollections;

public interface IResultContainer<TResult> where TResult : class
{
    IReadOnlyList<TResult> AsCollection();

    TResult? AsSingle();
}

public readonly struct ResultCollection<TResult> : IResultContainer<TResult> where TResult : class
{
    private readonly IReadOnlyList<TResult> _results;

    public ResultCollection(IReadOnlyList<TResult> results)
    {
        _results = results;
    }

    public IReadOnlyList<TResult> AsCollection() => _results;

    public TResult? AsSingle()
    {
        if (_results.Count > 0)
        {
            return _results[0];
        }
        return null;
    }
}

public readonly struct ResultElement<TResult> : IResultContainer<TResult> where TResult : class
{
    private readonly TResult? _result;

    public ResultElement(TResult? result)
    {
        _result = result;
    }

    public IReadOnlyList<TResult> AsCollection()
    {
        if (_result is not null)
        {
            return new[] { _result };
        }
        return Array.Empty<TResult>();
    }

    public TResult? AsSingle() => _result;
}
