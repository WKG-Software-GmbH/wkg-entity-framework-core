using Microsoft.EntityFrameworkCore.Metadata;

namespace Wkg.EntityFrameworkCore.Configuration.Policies.Defaults.EntityNamingPolicies.Internals;

internal readonly struct AllowImplicitNamingPolicy : IEntityNamingPolicy
{
    public void Audit(IMutableEntityType entityType) => Pass();
}