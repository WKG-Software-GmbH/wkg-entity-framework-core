namespace Wkg.EntityFrameworkCore.Configuration.Policies.ColumnNamingPolicies;

/// <summary>
/// Represents a column naming policy.
/// </summary>
public enum ColumnNamingPolicy
{
    /// <summary>
    /// Implicit column naming using EF Core conventions is allowed.
    /// </summary>
    AllowImplicit,

    /// <summary>
    /// Explicit column naming is preferred over implicit naming. Warnings will be generated for implicit naming.
    /// </summary>
    PreferExplicit,

    /// <summary>
    /// Only explicit column naming is allowed. Exceptions will be thrown for implicit naming.
    /// </summary>
    RequireExplicit
}
