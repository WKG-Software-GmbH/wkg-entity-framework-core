﻿using Microsoft.EntityFrameworkCore.Metadata;
using Wkg.Logging;

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
                Log.WriteDebug($"Property {implicitProperty.Name} was not explicitly mapped and was not marked as unmapped! {nameof(IgnoreImplicitMappingPolicy)} is active and will prevent convention-based mapping of this property.");
                if (implicitProperty.IsIndex())
                {
                    Log.WriteWarning($"Property {implicitProperty.Name} is an indexed property and will be removed from the containing index(es).");
                    // take a copy of the indexes to avoid modifying the collection while iterating
                    IMutableIndex[] indexes = implicitProperty.GetContainingIndexes().ToArray();
                    foreach (IMutableIndex index in indexes)
                    {
                        entityType.RemoveIndex(index);
                    }
                }
                if (implicitProperty.IsForeignKey())
                {
                    Log.WriteWarning($"Property {implicitProperty.Name} is a foreign key property and will be removed from the containing foreign key(s).");
                    // take a copy of the foreign keys to avoid modifying the collection while iterating
                    IMutableForeignKey[] fkeys = implicitProperty.GetContainingForeignKeys().ToArray();
                    foreach (IMutableForeignKey fkey in fkeys)
                    {
                        entityType.RemoveForeignKey(fkey);
                    }
                }
                entityType.RemoveProperty(implicitProperty);
                entityType.AddIgnored(implicitProperty.Name);
            }
        }
    }
}
