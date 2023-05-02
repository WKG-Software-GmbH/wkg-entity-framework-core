using Microsoft.EntityFrameworkCore.Metadata;

namespace Wkg.EntityFrameworkCore.Configuration.Reflection.Policies.MappingPolicies;

internal readonly struct AllowImplicitMappingPolicy : IMappingPolicy
{
    public void Audit(IMutableEntityType entityType) => Pass();
}