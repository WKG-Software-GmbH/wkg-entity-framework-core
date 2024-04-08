using Wkg.EntityFrameworkCore.Configuration.Policies.MappingPolicies;

namespace Wkg.EntityFrameworkCore.Configuration.Policies.PropertyMappingPolicies;

public static class PolicyOptionBuilderExtensions
{
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

public enum PropertyMappingPolicy
{
    AllowImplicit,
    IgnoreImplicit,
    PreferExplicit,
    RequireExplicit
}
