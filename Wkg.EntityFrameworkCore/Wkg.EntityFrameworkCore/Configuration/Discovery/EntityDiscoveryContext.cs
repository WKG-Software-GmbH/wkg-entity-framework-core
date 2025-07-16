using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Wkg.Common.Extensions;
using Wkg.Logging;
using Wkg.EntityFrameworkCore.Configuration.Policies;
using System.Runtime.ExceptionServices;
using Microsoft.EntityFrameworkCore;
using Wkg.EntityFrameworkCore.Configuration.Reflection.Discovery;
using System.Runtime.CompilerServices;
using Wkg.EntityFrameworkCore.Configuration.Reflection;

namespace Wkg.EntityFrameworkCore.Configuration.Discovery;

/// <inheritdoc cref="IEntityDiscoveryContext"/>
/// <summary>
/// Initializes a new instance of the <see cref="EntityDiscoveryContext"/> class using the specified <paramref name="policies"/>.
/// </summary>
/// <param name="policies">The policies to apply to enforce on discovered entities.</param>
public class EntityDiscoveryContext(IEntityPolicy[] policies) : IReflectiveEntityDiscoveryContext
{
    private static readonly ConditionalWeakTable<ModelBuilder, HashSet<Type>?> s_loadedDatabaseEngines = [];
    private readonly Dictionary<Type, IReflectiveEntityLoader> _loaders = [];

    IDictionary<Type, EntityTypeBuilder> IEntityDiscoveryContext.EntityBuilderCache { get; } = new Dictionary<Type, EntityTypeBuilder>();

    /// <inheritdoc/>
    public IEntityPolicy[] Policies => policies;

    /// <inheritdoc/>
    public void AuditPolicies()
    {
        IEntityDiscoveryContext self = this.To<IEntityDiscoveryContext>();

        // audit for compliance with the specified policies
        Log.WriteInfo($"Auditing {self.EntityBuilderCache.Count} entities for compliance with the specified policies.");
        List<Exception>? exceptions = null;
        foreach (EntityTypeBuilder entityType in self.EntityBuilderCache.Values)
        {
            foreach (IEntityPolicy policy in policies)
            {
                try
                {
                    policy.Audit(entityType.Metadata);
                }
                catch (PolicyViolationException e)
                {
                    Log.WriteException(e, "Policy validation failed.");
                    exceptions ??= [];
                    exceptions.Add(e);
                }
                catch (Exception e)
                {
                    Log.WriteException(e, LogLevel.Fatal);
                    throw;
                }
            }
        }
        if (exceptions is not null)
        {
            AggregateException aggregate = new("One or more policies could not be enforced.", exceptions);
            ExceptionDispatchInfo.SetCurrentStackTrace(aggregate);
            Log.WriteException(aggregate, LogLevel.Fatal);
            throw aggregate;
        }
        Log.WriteInfo($"Audit completed. {self.EntityBuilderCache.Count} entities loaded and configured.");
    }

    void IReflectiveEntityDiscoveryContext.AddLoader(IReflectiveEntityLoader loader) => _loaders.Add(loader.GetType(), loader);

    void IReflectiveDiscoveryContext.Discover(ModelBuilder builder, DiscoveryOptions options)
    {
        if (s_loadedDatabaseEngines.TryGetValue(builder, out HashSet<Type>? loadedDatabaseEngines))
        {
            // this ORM model builder has already been configured previously
            // null means that all database engines have been loaded
            if (loadedDatabaseEngines is null)
            {
                throw new InvalidOperationException("ORM model builder has already been configured for all reflectively loaded entities.");
            }
            Type[] dbEngineModelAttributeTypes = options.TargetDatabaseEngineAttributes;
            if (options.TargetDatabaseEngineAttributes.Length == 0)
            {
                throw new InvalidOperationException($"Cannot configure ORM model builder for all reflectively loaded entities, since it has already been configured to target specific database engines: {string.Join(", ", loadedDatabaseEngines.Select(t => t.Name))}.");
            }
            foreach (Type type in dbEngineModelAttributeTypes)
            {
                if (!loadedDatabaseEngines.Add(type))
                {
                    throw new InvalidOperationException($"The database engine {type.Name} has already been loaded.");
                }
                Log.WriteInfo($"Added discovery target for entities decorated with {type.Name}.");
            }
        }
        else
        {
            // this ORM model builder has not been configured previously
            loadedDatabaseEngines = options.TargetDatabaseEngineAttributes.Length == 0 ? null : [];
            s_loadedDatabaseEngines.Add(builder, loadedDatabaseEngines);
        }
        foreach (IReflectiveEntityLoader loader in _loaders.Values)
        {
            loader.LoadEntities(builder, this, options);
        }
    }
}
