using System.Data.Common;
using System.Linq.Expressions;

namespace Wkg.EntityFrameworkCore.ProcedureMapping.Compiler.Output;

public readonly record struct CompiledResultColumn
(
    string ColumnName, 
    string PropertyName, 
    Type PropertyClrType, 
    Expression ColumnFactory
);