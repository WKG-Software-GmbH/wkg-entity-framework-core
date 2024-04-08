using Wkg.Common.Extensions;
using Wkg.EntityFrameworkCore.Configuration.Policies.ColumnNamingPolicies.Internals;

namespace Wkg.EntityFrameworkCore.Configuration.Policies.ColumnNamingPolicies;

/// <summary>
/// Configures the naming policy for database columns.
/// </summary>
public class ColumnNaming : IEntityPolicyBuilder<ColumnNaming>
{
    private IColumnNamingPolicy _policy;
    private bool _policySet;

    private ColumnNaming()
    {
        _policy = new RequireExplicitNamingPolicy();
    }

    static ColumnNaming IEntityPolicyBuilder<ColumnNaming>.Create() => new();

    static bool IEntityPolicyBuilder.AllowMultiple => false;

    /// <summary>
    /// Allows implicit naming of database columns determined automatically from the corresponding property name via EF Core conventions.
    /// </summary>
    public void AllowImplicit()
    {
        AssertPolicyNotSet();
        _policy = new AllowImplicitNamingPolicy();
        _policySet = true;
    }

    /// <summary>
    /// Allows implicit naming of database columns determined automatically from the corresponding property name via EF Core conventions but generates warnings and advises against implicit naming.
    /// </summary>
    public void PreferExplicit()
    {
        AssertPolicyNotSet();
        _policy = new PreferExplicitNamingPolicy();
        _policySet = true;
    }

    /// <summary>
    /// Requires explicit naming of database columns and throws an exception if no explicit backing database column name is provided.
    /// </summary>
    public void RequireExplicit()
    {
        AssertPolicyNotSet();
        _policy = new RequireExplicitNamingPolicy();
        _policySet = true;
    }

    private void AssertPolicyNotSet()
    {
        if (_policySet)
        {
            throw new InvalidOperationException("A naming policy has already been set.");
        }
    }

    IEntityPolicy IEntityPolicyBuilder.Build() => _policy;

    internal static void AddDefaults(IPolicyOptionBuilder builder)
    {
        if (!builder.Contains<ColumnNaming>())
        {
            builder.AddPolicy<ColumnNaming>(Pass);
        }
    }
}