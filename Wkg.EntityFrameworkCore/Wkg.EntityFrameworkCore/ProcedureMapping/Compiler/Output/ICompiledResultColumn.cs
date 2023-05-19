using System.Data.Common;
using System.Linq.Expressions;

namespace Wkg.EntityFrameworkCore.ProcedureMapping.Compiler.Output;

/// <summary>
/// Represents the compiled bindings of a result column returned by a stored procedure.
/// </summary>
/// <param name="ColumnName">The name of the column.</param>
/// <param name="PropertyName">The name of the property mapped to the column.</param>
/// <param name="PropertyClrType">The CLR type of the property mapped to the column.</param>
/// <param name="ColumnFactory">The factory expression used to read the column value from the <see cref="DbDataReader"/>.</param>
public readonly record struct CompiledResultColumn
(
    string ColumnName, 
    string PropertyName, 
    Type PropertyClrType, 
    Expression ColumnFactory
);