using Wkg.EntityFrameworkCore.Configuration.Policies.MappingPolicies;

namespace Wkg.EntityFrameworkCore.Configuration.Policies.PropertyMappingPolicies;

/// <summary>
/// Provides extension methods for configuring property mapping policies.
/// </summary>
public static class PolicyOptionBuilderExtensions
{
    /// <summary>
    /// Adds the specified property mapping policy to the policy options.
    /// </summary>
    /// <param name="builder">The <see cref="IPolicyOptionBuilder"/> to configure.</param>
    /// <param name="propertyMapping">The <see cref="PropertyMappingPolicy"/> to add to the policy options.</param>
    /// <returns>The same <see cref="IPolicyOptionBuilder"/> instance for method chaining.</returns>
    /// <exception cref="ArgumentOutOfRangeException">When the <paramref name="propertyMapping"/> is not a valid <see cref="PropertyMappingPolicy"/> enum member.</exception>
    public static IPolicyOptionBuilder AddPropertyMappingPolicy(this IPolicyOptionBuilder builder, PropertyMappingPolicy propertyMapping) => 
        builder.AddPolicy<PropertyMapping>(policy => _ = propertyMapping switch
        {
            PropertyMappingPolicy.AllowImplicit => Do(policy.AllowImplicit),
            PropertyMappingPolicy.IgnoreImplicit => Do(policy.IgnoreImplicit),
            PropertyMappingPolicy.PreferExplicit => Do(policy.PreferExplicit),
            PropertyMappingPolicy.RequireExplicit => Do(policy.RequireExplicit),
            _ => throw new ArgumentOutOfRangeException(nameof(propertyMapping), propertyMapping, "Invalid property mapping policy.")
        });
}

/// <summary>
/// Represents the policy for configuring property mapping.
/// </summary>
public enum PropertyMappingPolicy
{
    /// <summary>
    /// Implicitly mapped properties automatically discovered by EF Core are allowed.
    /// </summary>
    AllowImplicit,

    /// <summary>
    /// Implicitly mapped properties automatically discovered by EF Core are ignored and removed from the model.
    /// </summary>
    IgnoreImplicit,

    /// <summary>
    /// Implicitly mapped properties automatically discovered by EF Core are allowed, but discouraged through warnings.
    /// </summary>
    PreferExplicit,

    /// <summary>
    /// Implicitly mapped properties automatically discovered by EF Core are disallowed and exceptions are thrown if they are encountered.
    /// </summary>
    RequireExplicit
}
