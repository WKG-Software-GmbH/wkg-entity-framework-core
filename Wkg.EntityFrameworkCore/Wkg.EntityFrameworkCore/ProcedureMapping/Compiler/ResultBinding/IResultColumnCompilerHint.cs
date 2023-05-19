using System.Data.Common;
using System.Linq.Expressions;

namespace Wkg.EntityFrameworkCore.ProcedureMapping.Compiler.ResultBinding;

/// <summary>
/// Represents optional information about a result column that can be used by the compiler to optimize the result binding.
/// </summary>
public interface IResultColumnCompilerHint
{
    /// <summary>
    /// Specifies the expression used to read the value of the column from the <see cref="DbDataReader"/>.
    /// </summary>
    public Expression ReaderGetExpression { get; }

    /// <summary>
    /// Specifies the type of the value read from the <see cref="DbDataReader"/>.
    /// </summary>
    public Type ReaderResultType { get; }

    /// <summary>
    /// Specifies the automatically determined expression used to convert the value read from the <see cref="DbDataReader"/> to the CLR type of the result column.
    /// </summary>
    /// <remarks>
    /// <para>
    /// The value of this property is <see langword="null"/> if the value read from the <see cref="DbDataReader"/> can be directly assigned to the CLR type of the result column, or if no automatic conversion is possible.
    /// </para>
    /// <para>
    /// The automatic conversion may be overridden by a user-supplied conversion expression.
    /// </para>
    /// </remarks>
    public Expression? AutoConversion { get; }
}