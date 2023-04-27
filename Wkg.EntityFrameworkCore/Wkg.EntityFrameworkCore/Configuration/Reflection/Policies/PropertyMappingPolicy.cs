using Wkg.EntityFrameworkCore.Configuration.Reflection.Policies.MappingPolicies;

namespace Wkg.EntityFrameworkCore.Configuration.Reflection.Policies;

public static class PropertyMappingPolicy
{
    public static IMappingPolicy AllowImplicit => new AllowImplicitMappingPolicy();
    public static IMappingPolicy PreferExplicit => new PreferExplicitMappingPolicy();
    public static IMappingPolicy RequireExplicit => new RequireExplicitMappingPolicy();
    public static IMappingPolicy IgnoreImplicit => new IgnoreImplicitMappingPolicy();
}