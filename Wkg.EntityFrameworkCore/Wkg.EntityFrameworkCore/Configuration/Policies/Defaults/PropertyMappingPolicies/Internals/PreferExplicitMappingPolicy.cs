using Microsoft.EntityFrameworkCore.Metadata;
using System.ComponentModel.DataAnnotations.Schema;
using Wkg.Logging;

namespace Wkg.EntityFrameworkCore.Configuration.Policies.Defaults.PropertyMappingPolicies.Internals;

internal readonly struct PreferExplicitMappingPolicy : IMappingPolicy
{
    public void Audit(IMutableEntityType entityType)
    {
        IMutableProperty[] unmappedProperties = entityType
            .GetDeclaredProperties()
            .Where(p => !p.GetAnnotations().Any())
            .ToArray();
        if (unmappedProperties.Length > 0)
        {
            Log.WriteWarning($"{nameof(PreferExplicitMappingPolicy)}: entity {entityType.ClrType.Name} contains implicitly mapped properties.");
            foreach (IMutableProperty implicitProperty in unmappedProperties)
            {
                Log.WriteWarning($"Property {implicitProperty.Name} was not explicitly mapped and was not marked as unmapped. **EF Core WILL ATTEMPT TO MAP THIS PROPERTY based on naming convention**! Consider using the [{nameof(NotMappedAttribute)}] or the Ignore() method if you do not want to map this property.");
            }
        }
    }
}