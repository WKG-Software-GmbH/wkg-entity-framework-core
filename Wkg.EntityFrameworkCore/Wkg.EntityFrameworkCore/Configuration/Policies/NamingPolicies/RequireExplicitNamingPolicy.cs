using Microsoft.EntityFrameworkCore.Metadata;

namespace Wkg.EntityFrameworkCore.Configuration.Policies.NamingPolicies;

internal class RequireExplicitNamingPolicy : ExplicitNamingPolicy
{
    public override void Audit(IMutableEntityType entityType)
    {
        if (!HasExplicitTableName(entityType))
        {
            throw new ArgumentException($"{nameof(RequireExplicitNamingPolicy)}: entity {entityType.ClrType.Name} was not explicitly mapped to a table or view.");
        }
        IMutableProperty[] implicitProperties = GetImplicitProperties(entityType);
        if (implicitProperties.Length > 0)
        {
            List<Exception> invalidPropertyExceptions = [];
            foreach (IMutableProperty implicitProperty in implicitProperties)
            {
                invalidPropertyExceptions.Add(new ArgumentException($"Property {implicitProperty.Name} has no explicit column name configured. Use HasColumnName() to specify a column name."));
            }
            throw new AggregateException($"{nameof(RequireExplicitNamingPolicy)}: entity {entityType.ClrType.Name} contains implicitly named properties.", invalidPropertyExceptions);
        }
    }
}