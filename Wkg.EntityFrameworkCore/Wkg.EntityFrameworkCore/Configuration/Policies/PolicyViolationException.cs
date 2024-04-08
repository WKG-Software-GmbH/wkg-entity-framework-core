namespace Wkg.EntityFrameworkCore.Configuration.Policies;

public class PolicyViolationException : Exception
{
    public PolicyViolationException(string? message = null) : base(message) => Pass();

    public PolicyViolationException(string? message, Exception? innerException) : base(message, innerException) => Pass();
}
