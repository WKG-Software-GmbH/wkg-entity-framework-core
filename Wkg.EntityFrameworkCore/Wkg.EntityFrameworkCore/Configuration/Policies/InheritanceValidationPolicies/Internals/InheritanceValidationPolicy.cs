﻿using Microsoft.EntityFrameworkCore.Metadata;

namespace Wkg.EntityFrameworkCore.Configuration.Policies.InheritanceValidationPolicies.Internals;

internal class InheritanceValidationPolicy(InheritanceValidationOptions options) : IEntityPolicy
{
    public void Audit(IMutableEntityType entityType)
    {
        if (!entityType.ClrType.IsAssignableTo(options.RequiredBaseType)
            && !options.ExceptedTypes.Contains(entityType.ClrType)
            && !options.ExceptedBaseTypes.Any(exception => entityType.ClrType.IsAssignableTo(exception)))
        {
            options.OnValidationFailure(entityType.ClrType);
        }
    }
}