namespace Wkg.EntityFrameworkCore.Configuration.Policies.Defaults.EntityNamingPolicies;

/// <summary>
/// Represents a naming policy for database components.
/// </summary>
public enum EntityNamingPolicy
{
    /// <summary>
    /// Implicit naming using EF Core conventions is allowed.
    /// </summary>
    AllowImplicit,

    /// <summary>
    /// Explicit naming is preferred over implicit naming. Warnings will be generated for implicit naming.
    /// </summary>
    PreferExplicit,

    /// <summary>
    /// Only explicit naming is allowed. Exceptions will be thrown for implicit naming.
    /// </summary>
    RequireExplicit
}
