namespace Wkg.EntityFrameworkCore.Configuration.Policies.Defaults.EntityNamingPolicies;

/// <summary>
/// Provides extension methods for configuring naming policies.
/// </summary>
public static class PolicyOptionBuilderExtensions
{
    /// <summary>
    /// Adds a naming policy to the policy options.
    /// </summary>
    /// <param name="builder">The <see cref="IPolicyOptionsBuilder"/> to configure.</param>
    /// <param name="namingPolicy">The <see cref="EntityNamingPolicy"/> to add to the policy options.</param>
    /// <returns>The same <see cref="IPolicyOptionsBuilder"/> instance for method chaining.</returns>
    /// <exception cref="ArgumentOutOfRangeException">When the <paramref name="namingPolicy"/> is not a valid <see cref="EntityNamingPolicy"/> enum member.</exception>
    public static IPolicyOptionsBuilder AddEntityNamingPolicy(this IPolicyOptionsBuilder builder, EntityNamingPolicy namingPolicy) =>
        builder.AddPolicy<EntityNaming>(policy => _ = namingPolicy switch
        {
            EntityNamingPolicy.AllowImplicit => Do(policy.AllowImplicit),
            EntityNamingPolicy.PreferExplicit => Do(policy.PreferExplicit),
            EntityNamingPolicy.RequireExplicit => Do(policy.RequireExplicit),
            _ => throw new ArgumentOutOfRangeException(nameof(namingPolicy), namingPolicy, "Invalid naming policy.")
        });
}
