using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Wkg.EntityFrameworkCore.Configuration.Policies;

namespace Wkg.EntityFrameworkCore.Configuration.Discovery;

/// <summary>
/// A context for entity discovery that can be used to enforce naming and mapping policies on discovered entities.
/// </summary>
public interface IEntityDiscoveryContext
{
    internal IDictionary<Type, EntityTypeBuilder> EntityBuilderCache { get; }

    /// <summary>
    /// The policies to enforce on discovered entities.
    /// </summary>
    IEntityPolicy[] Policies { get; }

    /// <summary>
    /// Audits all discovered entities for compliance with the policies and takes corresponding actions if necessary.
    /// </summary>
    void AuditPolicies();
}
