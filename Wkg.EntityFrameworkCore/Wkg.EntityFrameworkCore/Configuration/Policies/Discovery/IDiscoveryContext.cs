using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Wkg.EntityFrameworkCore.Configuration.Policies.MappingPolicies;
using Wkg.EntityFrameworkCore.Configuration.Policies.NamingPolicies;

namespace Wkg.EntityFrameworkCore.Configuration.Policies.Discovery;

/// <summary>
/// A context for entity discovery that can be used to enforce naming and mapping policies on discovered entities.
/// </summary>
public interface IDiscoveryContext
{
    /// <summary>
    /// The <see cref="INamingPolicy"/> policy to apply to discovered entities.
    /// </summary>
    INamingPolicy NamingPolicy { get; }

    /// <summary>
    /// The <see cref="IMappingPolicy"/> policy to apply to discovered entities.
    /// </summary>
    IMappingPolicy MappingPolicy { get; }

    /// <summary>
    /// Audits all discovered entities for compliance with the specified <see cref="NamingPolicy"/> and <see cref="MappingPolicy"/>.
    /// </summary>
    void AuditPolicies();

    internal IDictionary<Type, EntityTypeBuilder> EntityBuilderCache { get; }
}
