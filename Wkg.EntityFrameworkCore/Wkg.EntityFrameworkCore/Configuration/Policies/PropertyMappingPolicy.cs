using Wkg.EntityFrameworkCore.Configuration.Policies.MappingPolicies;
using Wkg.EntityFrameworkCore.Configuration.Reflection.Policies.MappingPolicies;

namespace Wkg.EntityFrameworkCore.Configuration.Policies;

/// <summary>
/// Provides a set of default <see cref="IMappingPolicy"/> implementations.
/// </summary>
public static class PropertyMappingPolicy
{
    /// <summary>
    /// Allows implicit, convention-based mapping of properties.
    /// </summary>
    public static IMappingPolicy AllowImplicit => new AllowImplicitMappingPolicy();

    /// <summary>
    /// Prefers explicit mapping of properties, but allows implicit, convention-based mapping and generates a warning if such a mapping is found.
    /// </summary>
    public static IMappingPolicy PreferExplicit => new PreferExplicitMappingPolicy();

    /// <summary>
    /// Requires explicit mapping of every property and generates an error if such a mapping is not found.
    /// </summary>
    public static IMappingPolicy RequireExplicit => new RequireExplicitMappingPolicy();

    /// <summary>
    /// Automatically ignores properties that are not explicitly mapped.
    /// </summary>
    public static IMappingPolicy IgnoreImplicit => new IgnoreImplicitMappingPolicy();
}