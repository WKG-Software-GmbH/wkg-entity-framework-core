using Microsoft.EntityFrameworkCore.Metadata;
using System.ComponentModel.DataAnnotations.Schema;

namespace Wkg.EntityFrameworkCore.Configuration.Policies.MappingPolicies;

internal readonly struct RequireExplicitMappingPolicy : IMappingPolicy
{
    public void Audit(IMutableEntityType entityType)
    {
        IMutableProperty[] unmappedProperties = entityType
            .GetDeclaredProperties()
            .Where(p => !p.GetAnnotations().Any())
            .ToArray();
        if (unmappedProperties.Length > 0)
        {
            List<Exception> unmappedPropertyExceptions = new();
            foreach (IMutableProperty implicitProperty in unmappedProperties)
            {
                unmappedPropertyExceptions.Add(new ArgumentException($"Property '{implicitProperty.Name}' was not explicitly mapped and was not marked as unmapped. **EF Core WILL ATTEMPT TO MAP THIS PROPERTY based on naming convention**! Consider using the [{nameof(NotMappedAttribute)}] or the Ignore() method if you do not want to map this property."));
            }
            throw new AggregateException($"{nameof(RequireExplicitMappingPolicy)}: entity '{entityType.ClrType.Name}' contains implicitly mapped properties.", unmappedPropertyExceptions);
        }
    }
}
