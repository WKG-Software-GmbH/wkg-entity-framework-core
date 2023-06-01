using System.Data.Common;

namespace Wkg.EntityFrameworkCore.ProcedureMapping.Compiler.Output;

/// <summary>
/// Represents a dynamically compiled delegate that can be used to construct a result entity from a next result row of the provided <typeparamref name="TReader"/> <see cref="DbDataReader"/> instance.
/// </summary>
/// <typeparam name="TReader">The type of the <see cref="DbDataReader"/> instance.</typeparam>
/// <param name="reader">The <see cref="DbDataReader"/> instance to read the next result row from.</param>
/// <returns>The constructed result entity.</returns>
public delegate object CompiledResultFactory<TReader>(TReader reader) where TReader : DbDataReader;