using Wkg.EntityFrameworkCore.Configuration.Reflection.Policies.NamingPolicies;

namespace Wkg.EntityFrameworkCore.Configuration.Reflection.Policies;

public static class EntityNamingPolicy
{
    public static INamingPolicy AllowImplicit => new AllowImplicitNamingPolicy();
    public static INamingPolicy PreferExplicit => new PreferExplicitNamingPolicy();
    public static INamingPolicy RequireExplicit => new RequireExplicitNamingPolicy();
}