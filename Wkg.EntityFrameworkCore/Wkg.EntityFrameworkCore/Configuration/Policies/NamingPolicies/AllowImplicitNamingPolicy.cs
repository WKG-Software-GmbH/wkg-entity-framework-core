using Microsoft.EntityFrameworkCore.Metadata;

namespace Wkg.EntityFrameworkCore.Configuration.Policies.NamingPolicies;

internal readonly struct AllowImplicitNamingPolicy : INamingPolicy
{
    public void Audit(IMutableEntityType entityType) => Pass();
}