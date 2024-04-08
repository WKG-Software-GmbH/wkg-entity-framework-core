using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Wkg.Common.Extensions;
using Wkg.Logging;
using Wkg.EntityFrameworkCore.Configuration.Policies;

namespace Wkg.EntityFrameworkCore.Configuration.Discovery;

/// <inheritdoc cref="IDiscoveryContext"/>
/// <summary>
/// Initializes a new instance of the <see cref="EntityDiscoveryContext"/> class using the specified <paramref name="policies"/>.
/// </summary>
/// <param name="policies">The policies to apply to enforce on discovered entities.</param>
internal class EntityDiscoveryContext(IEntityPolicy[] policies) : IDiscoveryContext
{
    IDictionary<Type, EntityTypeBuilder> IDiscoveryContext.EntityBuilderCache { get; } = new Dictionary<Type, EntityTypeBuilder>();

    public IEntityPolicy[] Policies => policies;

    /// <inheritdoc/>
    public void AuditPolicies()
    {
        IDiscoveryContext self = this.To<IDiscoveryContext>();

        // audit for compliance with the specified policies
        Log.WriteInfo($"Auditing {self.EntityBuilderCache.Count} entities for compliance with the specified policies.");
        foreach (EntityTypeBuilder entityType in self.EntityBuilderCache.Values)
        {
            foreach (IEntityPolicy policy in policies)
            {
                policy.Audit(entityType.Metadata);
            }
        }
        Log.WriteInfo($"Audit completed. {self.EntityBuilderCache.Count} entities loaded and configured.");
    }
}
