namespace Wkg.EntityFrameworkCore.Configuration.Policies;

/// <summary>
/// Extension methods for <see cref="IPolicyOptionsBuilder"/>.
/// </summary>
public static class PolicyOptionsBuilderExtensions
{
    /// <summary>
    /// Adds a policy to the builder if it does not already exist.
    /// </summary>
    /// <typeparam name="TPolicyBuilder">The type of the policy builder.</typeparam>
    /// <param name="builder">The <see cref="IPolicyOptionsBuilder"/> to configure.</param>
    /// <returns><see langword="true"/> if the policy was added; otherwise, <see langword="false"/>.</returns>
    public static bool TryAddPolicy<TPolicyBuilder>(this IPolicyOptionsBuilder builder) where TPolicyBuilder : class, IEntityPolicyBuilder<TPolicyBuilder>
    {
        if (!builder.Contains<TPolicyBuilder>())
        {
            builder.AddPolicy<TPolicyBuilder>();
            return true;
        }
        return false;
    }
}
