namespace Wkg.EntityFrameworkCore.ProcedureMapping.Builder.ThrowHelpers;

/// <summary>
/// Represents an exception that is thrown when an error occurs during stored procedure configuration.
/// </summary>
/// <remarks>
/// Initializes a new instance of the <see cref="ProcedureBuilderException"/> class with the specified message.
/// </remarks>
/// <param name="message">The message that describes the error.</param>
public class ProcedureBuilderException(string message) : Exception(message);