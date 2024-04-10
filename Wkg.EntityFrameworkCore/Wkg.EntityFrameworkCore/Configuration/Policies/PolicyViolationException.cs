namespace Wkg.EntityFrameworkCore.Configuration.Policies;

/// <summary>
/// Represents an exception that is thrown when a policy violation occurs.
/// </summary>
public class PolicyViolationException : Exception
{
    /// <summary>
    /// Initializes a new instance of the <see cref="PolicyViolationException"/> class with the specified optional <paramref name="message"/>.
    /// </summary>
    /// <param name="message">The message that describes the error.</param>
    public PolicyViolationException(string? message = null) : base(message) => Pass();

    /// <summary>
    /// Initializes a new instance of the <see cref="PolicyViolationException"/> class with the specified <paramref name="message"/> and <paramref name="innerException"/>.
    /// </summary>
    /// <param name="message">The message that describes the error.</param>
    /// <param name="innerException">The exception that is the cause of the current exception.</param>
    public PolicyViolationException(string? message, Exception? innerException) : base(message, innerException) => Pass();
}
