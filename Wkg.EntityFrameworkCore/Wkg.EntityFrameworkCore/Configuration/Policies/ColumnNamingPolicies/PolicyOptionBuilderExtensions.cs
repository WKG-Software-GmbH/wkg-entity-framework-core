namespace Wkg.EntityFrameworkCore.Configuration.Policies.ColumnNamingPolicies;

/// <summary>
/// Provides extension methods for configuring column naming policies.
/// </summary>
public static class PolicyOptionBuilderExtensions
{
    /// <summary>
    /// Adds a column naming policy to the policy options.
    /// </summary>
    /// <param name="builder">The <see cref="IPolicyOptionBuilder"/> to configure.</param>
    /// <param name="namingPolicy">The <see cref="ColumnNamingPolicy"/> to add to the policy options.</param>
    /// <returns>The same <see cref="IPolicyOptionBuilder"/> instance for method chaining.</returns>
    /// <exception cref="ArgumentOutOfRangeException">When the <paramref name="namingPolicy"/> is not a valid <see cref="ColumnNamingPolicy"/> enum member.</exception>
    public static IPolicyOptionBuilder AddColumnNamingPolicy(this IPolicyOptionBuilder builder, ColumnNamingPolicy namingPolicy) => 
        builder.AddPolicy<ColumnNaming>(policy => _ = namingPolicy switch
        {
            ColumnNamingPolicy.AllowImplicit => Do(policy.AllowImplicit),
            ColumnNamingPolicy.PreferExplicit => Do(policy.PreferExplicit),
            ColumnNamingPolicy.RequireExplicit => Do(policy.RequireExplicit),
            _ => throw new ArgumentOutOfRangeException(nameof(namingPolicy), namingPolicy, "Invalid naming policy.")
        });
}
