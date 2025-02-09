using Microsoft.EntityFrameworkCore;
using Wkg.EntityFrameworkCore.Configuration.Reflection;
using Wkg.EntityFrameworkCore.Configuration.Reflection.Discovery;

namespace Wkg.EntityFrameworkCore.Configuration.Discovery;

/// <summary>
/// Represents the base class for a context that can be used to discover procedures using reflection.
/// </summary>
public abstract class ProcedureDiscoveryContext : IReflectiveProcedureDiscoveryContext
{
    private readonly Dictionary<Type, IReflectiveProcedureLoader> _loaders = [];

    /// <summary>
    /// A static cache of model builders that have already been configured. This is used to prevent duplicate configuration of the same model builder.
    /// </summary>
    protected abstract List<WeakReference<ModelBuilder>> StaticModelBuilderCache { get; }

    /// <inheritdoc/>
    public void Discover(ModelBuilder builder, DiscoveryOptions options)
    {
        List<WeakReference<ModelBuilder>> configuredModelBuilders = StaticModelBuilderCache;
        for (int i = configuredModelBuilders.Count - 1; i >= 0; i--)
        {
            if (configuredModelBuilders[i].TryGetTarget(out ModelBuilder? target))
            {
                if (ReferenceEquals(target, builder))
                {
                    throw new InvalidOperationException("ORM model has already been loaded.");
                }
            }
            else
            {
                configuredModelBuilders.RemoveAt(i);
            }
        }
        configuredModelBuilders.Add(new WeakReference<ModelBuilder>(builder));
        foreach (IReflectiveProcedureLoader loader in _loaders.Values)
        {
            loader.LoadProcedures(builder, this, options);
        }
    }

    void IReflectiveProcedureDiscoveryContext.AddLoader(IReflectiveProcedureLoader loader) => _loaders.Add(loader.GetType(), loader);
}
