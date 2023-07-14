using Wkg.EntityFrameworkCore.Configuration.Policies.NamingPolicies;
using Wkg.EntityFrameworkCore.Configuration.Reflection.Policies.NamingPolicies;

namespace Wkg.EntityFrameworkCore.Configuration.Policies;

/// <summary>
/// Provides a set of predefined <see cref="INamingPolicy"/> implementations.
/// </summary>
public static class NamingPolicy
{
    /// <summary>
    /// Allows implicit naming of database columns determined automatically from the corresponding property name via EF Core conventions.
    /// </summary>
    public static INamingPolicy AllowImplicit => new AllowImplicitNamingPolicy();

    /// <summary>
    /// Allows implicit naming of database columns determined automatically from the corresponding property name via EF Core conventions but generates warnings and advises against implicit naming.
    /// </summary>
    public static INamingPolicy PreferExplicit => new PreferExplicitNamingPolicy();

    /// <summary>
    /// Requires explicit naming of database columns and throws an exception if no explicit backing database column name is provided.
    /// </summary>
    public static INamingPolicy RequireExplicit => new RequireExplicitNamingPolicy();
}