using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Wkg.EntityFrameworkCore.Configuration.Policies.MappingPolicies;
using Wkg.EntityFrameworkCore.Configuration.Policies.NamingPolicies;
using Wkg.Extensions.Common;
using Wkg.Logging;

namespace Wkg.EntityFrameworkCore.Configuration.Policies.Discovery;

/// <inheritdoc cref="IDiscoveryContext"/>
internal class EntityDiscoveryContext : IDiscoveryContext
{
    /// <summary>
    /// Initializes a new instance of the <see cref="EntityDiscoveryContext"/> class using the specified <paramref name="namingPolicy"/> and <paramref name="mappingPolicy"/>.
    /// </summary>
    /// <param name="namingPolicy">The <see cref="INamingPolicy"/> to use. If <see langword="null"/>, <see cref="NamingPolicy.RequireExplicit"/> will be used.</param>
    /// <param name="mappingPolicy">The <see cref="IMappingPolicy"/> to use. If <see langword="null"/>, <see cref="PropertyMappingPolicy.IgnoreImplicit"/> will be used.</param>
    public EntityDiscoveryContext(INamingPolicy? namingPolicy, IMappingPolicy? mappingPolicy)
    {
        NamingPolicy = namingPolicy ?? Policies.NamingPolicy.RequireExplicit;
        MappingPolicy = mappingPolicy ?? PropertyMappingPolicy.IgnoreImplicit;
    }

    /// <inheritdoc/>
    public INamingPolicy NamingPolicy { get; }

    /// <inheritdoc/>
    public IMappingPolicy MappingPolicy { get; }

    IDictionary<Type, EntityTypeBuilder> IDiscoveryContext.EntityBuilderCache { get; } = new Dictionary<Type, EntityTypeBuilder>();

    /// <inheritdoc/>
    public void AuditPolicies()
    {
        IDiscoveryContext self = this.To<IDiscoveryContext>();

        // audit for compliance with the specified policies
        Log.WriteInfo($"Auditing {self.EntityBuilderCache.Count} entities for compliance with the specified policies.");
        foreach (EntityTypeBuilder entityType in self.EntityBuilderCache.Values)
        {
            self.MappingPolicy.Audit(entityType.Metadata);
            self.NamingPolicy.Audit(entityType.Metadata);
        }
        Log.WriteInfo($"Audit completed. {self.EntityBuilderCache.Count} entities loaded and configured.");
    }
}
