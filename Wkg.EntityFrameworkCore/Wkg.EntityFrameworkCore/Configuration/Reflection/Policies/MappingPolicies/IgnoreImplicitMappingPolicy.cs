using Microsoft.EntityFrameworkCore.Metadata;

namespace Wkg.EntityFrameworkCore.Configuration.Reflection.Policies.MappingPolicies;

internal readonly struct IgnoreImplicitMappingPolicy : IMappingPolicy
{
    public void Audit(IMutableEntityType entityType)
    {
        IMutableProperty[] unmappedProperties = entityType
            .GetDeclaredProperties()
            .Where(p => !p.GetAnnotations().Any())
            .ToArray();

        if (unmappedProperties.Length > 0)
        {
            foreach (IMutableProperty implicitProperty in unmappedProperties)
            {
                entityType.RemoveProperty(implicitProperty);
                entityType.AddIgnored(implicitProperty.Name);
            }
        }
    }
}
