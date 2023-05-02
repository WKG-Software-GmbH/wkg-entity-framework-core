﻿using Microsoft.EntityFrameworkCore.Metadata;

namespace Wkg.EntityFrameworkCore.Configuration.Reflection.Policies.NamingPolicies;

/// <summary>
/// A policy that can be applied to an <see cref="IMutableEntityType" /> to enforce a specific naming guideline. 
/// An <see cref="INamingPolicy"/> determines what action should be taken when no explicit colomn name is provided.
/// </summary>
public interface INamingPolicy : IPolicy
{
}