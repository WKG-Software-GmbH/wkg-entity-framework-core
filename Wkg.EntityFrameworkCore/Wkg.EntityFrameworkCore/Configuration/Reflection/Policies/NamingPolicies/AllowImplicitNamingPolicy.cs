using Microsoft.EntityFrameworkCore.Metadata;
using static Wkg.SyntacticSugar;

namespace Wkg.EntityFrameworkCore.Configuration.Reflection.Policies.NamingPolicies;

internal readonly struct AllowImplicitNamingPolicy : INamingPolicy
{
    public void Audit(IMutableEntityType entityType) => Pass();
}