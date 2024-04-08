using Microsoft.EntityFrameworkCore.Metadata;

namespace Wkg.EntityFrameworkCore.Configuration.Policies.ColumnNamingPolicies.Internals;

internal readonly struct AllowImplicitNamingPolicy : IColumnNamingPolicy
{
    public void Audit(IMutableEntityType entityType) => Pass();
}