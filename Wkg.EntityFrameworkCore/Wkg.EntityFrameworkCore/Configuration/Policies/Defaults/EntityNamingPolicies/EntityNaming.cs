using Wkg.EntityFrameworkCore.Configuration.Policies.Defaults.EntityNamingPolicies.Internals;

namespace Wkg.EntityFrameworkCore.Configuration.Policies.Defaults.EntityNamingPolicies;

/// <summary>
/// Configures the naming policy for database components.
/// </summary>
public class EntityNaming : IEntityPolicyBuilder<EntityNaming>
{
    private IEntityNamingPolicy _policy;
    private bool _policySet;

    private EntityNaming()
    {
        _policy = new RequireExplicitNamingPolicy();
    }

    static EntityNaming IEntityPolicyBuilder<EntityNaming>.Create() => new();

    static bool IEntityPolicyBuilder.AllowMultiple => false;

    /// <summary>
    /// Allows implicit naming of database components determined automatically from the corresponding property name via EF Core conventions.
    /// </summary>
    public void AllowImplicit()
    {
        AssertPolicyNotSet();
        _policy = new AllowImplicitNamingPolicy();
        _policySet = true;
    }

    /// <summary>
    /// Allows implicit naming of database components determined automatically from the corresponding property name via EF Core conventions but generates warnings and advises against implicit naming.
    /// </summary>
    public void PreferExplicit()
    {
        AssertPolicyNotSet();
        _policy = new PreferExplicitNamingPolicy();
        _policySet = true;
    }

    /// <summary>
    /// Requires explicit naming of database components and throws an exception if no explicit backing database column name is provided.
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

    internal static void AddDefaults(IPolicyOptionsBuilder builder) => builder.TryAddPolicy<EntityNaming>();
}