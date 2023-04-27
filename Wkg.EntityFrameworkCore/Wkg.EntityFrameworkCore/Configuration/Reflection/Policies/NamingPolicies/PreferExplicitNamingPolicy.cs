﻿using Microsoft.EntityFrameworkCore.Metadata;
using Wkg.EntityFrameworkCore.Extensions;

namespace Wkg.EntityFrameworkCore.Configuration.Reflection.Policies.NamingPolicies;

internal readonly struct PreferExplicitNamingPolicy : INamingPolicy
{
    public void Audit(IMutableEntityType entityType)
    {
        IMutableProperty[] implicitProperties = entityType
            .GetDeclaredProperties()
            .Where(p => !p.HasAnnotation(annotation => annotation is RelationalAnnotationNames.ColumnName))
            .ToArray();
        if (implicitProperties.Length > 0)
        {
            Console.WriteLine($"{nameof(PreferExplicitNamingPolicy)}: entity {entityType.ClrType.Name} contains implicitly named properties.");
            foreach (IMutableProperty implicitProperty in implicitProperties)
            {
                Console.WriteLine($"Property {implicitProperty.Name} has no explicit column name configured. Using the property name as column name. Consider using HasColumnName() to specify a column name.");
            }
        }
    }
}