using System.Linq.Expressions;

namespace Wkg.EntityFrameworkCore.ProcedureMapping.Compiler.ResultBinding;

public readonly record struct ResultColumnCompilerSettings
(
    Expression ReaderGetExpression, 
    Type ReaderResultType, 
    Expression? AutoConversion
) : IResultColumnCompilerHint;