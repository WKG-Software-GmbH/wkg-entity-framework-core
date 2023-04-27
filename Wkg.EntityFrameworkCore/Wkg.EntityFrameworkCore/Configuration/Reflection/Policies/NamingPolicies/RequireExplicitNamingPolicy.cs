using Microsoft.EntityFrameworkCore.Metadata;
using Wkg.EntityFrameworkCore.Extensions;

namespace Wkg.EntityFrameworkCore.Configuration.Reflection.Policies.NamingPolicies;

internal readonly struct RequireExplicitNamingPolicy : INamingPolicy
{
    public void Audit(IMutableEntityType entityType)
    {
        if (!entityType.HasAnnotation(annotation => annotation is RelationalAnnotationNames.TableName or RelationalAnnotationNames.ViewName))
        {
            throw new ArgumentException($"{nameof(RequireExplicitNamingPolicy)}: entity {entityType.ClrType.Name} was not explicitly mapped to a table or view.");
        }
        IMutableProperty[] implicitProperties = entityType
            .GetDeclaredProperties()
            .Where(p => !p.HasAnnotation(annotation => annotation is RelationalAnnotationNames.ColumnName))
            .ToArray();
        if (implicitProperties.Length > 0)
        {
            List<Exception> invalidPropertyExceptions = new();
            foreach (IMutableProperty implicitProperty in implicitProperties)
            {
                invalidPropertyExceptions.Add(new ArgumentException($"Property {implicitProperty.Name} has no explicit column name configured. Use HasColumnName() to specify a column name."));
            }
            throw new AggregateException($"{nameof(RequireExplicitNamingPolicy)}: entity {entityType.ClrType.Name} contains implicitly named properties.", invalidPropertyExceptions);
        }
    }
}