using Microsoft.EntityFrameworkCore.Metadata;

namespace Wkg.EntityFrameworkCore.Configuration.Policies.PropertyMappingPolicies.Internals;

internal readonly struct AllowImplicitMappingPolicy : IMappingPolicy
{
    public void Audit(IMutableEntityType entityType) => Pass();
}