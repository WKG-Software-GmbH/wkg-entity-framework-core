using Microsoft.EntityFrameworkCore.Metadata;
using Wkg.EntityFrameworkCore.Extensions;

namespace Wkg.EntityFrameworkCore.Configuration.Policies.Defaults.EntityNamingPolicies.Internals;

internal abstract class ExplicitNamingPolicy : IEntityNamingPolicy
{
    protected static bool HasExplicitTableName(IMutableEntityType entityType)
    {
        // allow TPH inheritance to inherit the table name from the base class
        for (IMutableEntityType? type = entityType; type is not null; type = type.BaseType)
        {
            if (type.HasAnnotation(annotation => annotation is RelationalAnnotationNames.TableName or RelationalAnnotationNames.ViewName))
            {
                return true;
            }
        }
        return false;
    }

    protected static IMutableProperty[] GetImplicitProperties(IMutableEntityType entityType) => entityType
        .GetDeclaredProperties()
        .Where(p => !p.HasAnnotation(annotation => annotation is RelationalAnnotationNames.ColumnName)
            // Ignore shadow properties (e.g. for TPT inheritance)
            && !p.IsShadowProperty())
        .ToArray();

    public abstract void Audit(IMutableEntityType entityType);
}
