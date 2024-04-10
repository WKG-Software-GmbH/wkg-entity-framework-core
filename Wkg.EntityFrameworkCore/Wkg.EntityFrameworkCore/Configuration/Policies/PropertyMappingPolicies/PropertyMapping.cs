using Wkg.EntityFrameworkCore.Configuration.Policies.MappingPolicies.Internals;

namespace Wkg.EntityFrameworkCore.Configuration.Policies.MappingPolicies;

/// <summary>
/// Configures the mapping policy for properties.
/// </summary>
public class PropertyMapping : IEntityPolicyBuilder<PropertyMapping>
{
    private IMappingPolicy _policy;
    private bool _policySet;

    private PropertyMapping()
    {
        _policy = new IgnoreImplicitMappingPolicy();
        _policySet = false;
    }

    static PropertyMapping IEntityPolicyBuilder<PropertyMapping>.Create() => new();

    static bool IEntityPolicyBuilder.AllowMultiple => false;

    /// <summary>
    /// Allows implicit, convention-based mapping of properties.
    /// </summary>
    public void AllowImplicit()
    {
        AssertPolicyNotSet();
        _policy = new AllowImplicitMappingPolicy();
        _policySet = true;
    }

    /// <summary>
    /// Prefers explicit mapping of properties, but allows implicit, convention-based mapping and generates a warning if such a mapping is found.
    /// </summary>
    public void PreferExplicit()
    {
        AssertPolicyNotSet();
        _policy = new PreferExplicitMappingPolicy();
        _policySet = true;
    }

    /// <summary>
    /// Requires explicit mapping of every property and generates an error if such a mapping is not found.
    /// </summary>
    public void RequireExplicit()
    {
        AssertPolicyNotSet();
        _policy = new RequireExplicitMappingPolicy();
        _policySet = true;
    }

    /// <summary>
    /// Automatically ignores properties that are not explicitly mapped.
    /// </summary>
    public void IgnoreImplicit()
    {
        AssertPolicyNotSet();
        _policy = new IgnoreImplicitMappingPolicy();
        _policySet = true;
    }

    IEntityPolicy IEntityPolicyBuilder.Build() => _policy;

    private void AssertPolicyNotSet()
    {
        if (_policySet)
        {
            throw new InvalidOperationException("The mapping policy has already been set.");
        }
    }

    internal static void AddDefaults(IPolicyOptionBuilder builder)
    {
        if (!builder.Contains<PropertyMapping>())
        {
            builder.AddPolicy<PropertyMapping>(Pass);
        }
    }
}