using Microsoft.EntityFrameworkCore.Metadata;
using System.ComponentModel.DataAnnotations.Schema;

namespace Wkg.EntityFrameworkCore.Configuration.Reflection.Policies.MappingPolicies;

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
            Console.WriteLine($"{nameof(PreferExplicitMappingPolicy)}: entity {entityType.ClrType.Name} contains implicitly mapped properties.");
            foreach (IMutableProperty implicitProperty in unmappedProperties)
            {
                Console.WriteLine($"Property {implicitProperty.Name} was not explicitly mapped and was not marked as unmapped! Consider using the [{nameof(NotMappedAttribute)}] or the Ignore() method if you do not want to map this property.");
            }
        }
    }
}