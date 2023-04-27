using System.Linq.Expressions;

namespace Wkg.EntityFrameworkCore.ProcedureMapping.Compiler.ResultBinding;

public interface IResultColumnCompilerHint
{
    public Expression ReaderGetExpression { get; }

    public Type ReaderResultType { get; }

    public Expression? AutoConversion { get; }
}