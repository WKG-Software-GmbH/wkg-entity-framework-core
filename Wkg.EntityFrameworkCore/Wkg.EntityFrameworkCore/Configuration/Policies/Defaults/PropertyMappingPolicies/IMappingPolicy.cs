using Microsoft.EntityFrameworkCore.Metadata;

namespace Wkg.EntityFrameworkCore.Configuration.Policies.Defaults.PropertyMappingPolicies;

/// <summary>
/// A policy that can be applied to an <see cref="IMutableEntityType" /> to enforce a specific mapping guideline.
/// An <see cref="IMappingPolicy"/> determines what action should be taken when a property is neither ignored nor explicitly mapped.
/// </summary>
public interface IMappingPolicy : IEntityPolicy;