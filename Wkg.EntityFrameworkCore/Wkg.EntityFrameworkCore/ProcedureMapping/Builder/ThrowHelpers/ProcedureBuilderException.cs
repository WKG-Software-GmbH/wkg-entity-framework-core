namespace Wkg.EntityFrameworkCore.ProcedureMapping.Builder.ThrowHelpers;

/// <summary>
/// Represents an exception that is thrown when an error occurs during stored procedure configuration.
/// </summary>
public class ProcedureBuilderException : Exception
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ProcedureBuilderException"/> class with the specified message.
    /// </summary>
    /// <param name="message">The message that describes the error.</param>
    public ProcedureBuilderException(string message) : base(message)
    {
    }
}
