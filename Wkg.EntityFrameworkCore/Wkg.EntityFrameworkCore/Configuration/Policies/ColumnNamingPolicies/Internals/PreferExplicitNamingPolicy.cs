using Microsoft.EntityFrameworkCore.Metadata;
using Wkg.Logging;

namespace Wkg.EntityFrameworkCore.Configuration.Policies.ColumnNamingPolicies.Internals;

internal class PreferExplicitNamingPolicy : ExplicitNamingPolicy
{
    public override void Audit(IMutableEntityType entityType)
    {
        if (!HasExplicitTableName(entityType))
        {
            Log.WriteWarning($"{nameof(PreferExplicitNamingPolicy)}: entity {entityType.ClrType.Name} was not explicitly mapped to a table or view.");
        }
        IMutableProperty[] implicitProperties = GetImplicitProperties(entityType);
        if (implicitProperties.Length > 0)
        {
            Log.WriteWarning($"{nameof(PreferExplicitNamingPolicy)}: entity {entityType.ClrType.Name} contains implicitly named properties.");
            foreach (IMutableProperty implicitProperty in implicitProperties)
            {
                Log.WriteWarning($"Property {implicitProperty.Name} has no explicit column name configured. EF Core will use the property name as column name. Consider using HasColumnName() to specify a column name.");
            }
        }
    }
}
