using Microsoft.EntityFrameworkCore.Metadata;

namespace Wkg.EntityFrameworkCore.Configuration.Policies.MappingPolicies.Internals;

internal readonly struct AllowImplicitMappingPolicy : IMappingPolicy
{
    public void Audit(IMutableEntityType entityType) => Pass();
}