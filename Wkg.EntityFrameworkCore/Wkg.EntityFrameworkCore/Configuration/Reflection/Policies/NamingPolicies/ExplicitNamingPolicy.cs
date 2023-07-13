using Microsoft.EntityFrameworkCore.Metadata;
using Wkg.EntityFrameworkCore.Extensions;

namespace Wkg.EntityFrameworkCore.Configuration.Reflection.Policies.NamingPolicies;

internal abstract class ExplicitNamingPolicy : INamingPolicy
{
    protected static bool HasExplicitTableName(IMutableEntityType entityType)
    {
        // allow TPH inheritance to inherit the table name from the base class
        List<IMutableEntityType> inheritanceChain = new(1);
        for (IMutableEntityType? type = entityType; type is not null; type = type.BaseType)
        {
            inheritanceChain.Add(type);
        }
        return inheritanceChain.Any(type => type.HasAnnotation(annotation => annotation is RelationalAnnotationNames.TableName or RelationalAnnotationNames.ViewName));
    }

    protected static IMutableProperty[] GetImplicitProperties(IMutableEntityType entityType) => entityType
        .GetDeclaredProperties()
        .Where(p => !p.HasAnnotation(annotation => annotation is RelationalAnnotationNames.ColumnName) 
            // Allow shadow properties to be implicitly mapped (e.g. for TPT inheritance)
            && !p.IsShadowProperty())
        .ToArray();

    public abstract void Audit(IMutableEntityType entityType);
}
