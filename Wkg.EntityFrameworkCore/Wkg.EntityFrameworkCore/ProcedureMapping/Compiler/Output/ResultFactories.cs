using System.Data.Common;

namespace Wkg.EntityFrameworkCore.ProcedureMapping.Compiler.Output;

public delegate object CompiledResultFactory<TReader>(TReader reader) where TReader : DbDataReader;