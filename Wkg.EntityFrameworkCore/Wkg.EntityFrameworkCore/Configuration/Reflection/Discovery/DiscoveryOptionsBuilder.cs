using System.Reflection;
using Wkg.EntityFrameworkCore.Configuration.Reflection.Attributes;

namespace Wkg.EntityFrameworkCore.Configuration.Reflection.Discovery;

/// <summary>
/// Configures global entity discovery options.
/// </summary>
public class DiscoveryOptionsBuilder : IDiscoveryOptionsBuilder
{
    private readonly HashSet<Assembly> _assemblies = [];
    private readonly HashSet<Type> _databaseEngines = [];

    /// <summary>
    /// Creates a new instance of <see cref="DiscoveryOptionsBuilder"/>.
    /// </summary>
    internal protected DiscoveryOptionsBuilder() => Pass();

    /// <inheritdoc />
    public virtual IDiscoveryOptionsBuilder AddTargetAssembly<TTargetAssembly>() where TTargetAssembly : class, ITargetAssembly
    {
        if (!_assemblies.Add(TTargetAssembly.Assembly))
        {
            throw new InvalidOperationException($"The assembly '{TTargetAssembly.Assembly.FullName}' has already been added.");
        }
        return this;
    }

    /// <inheritdoc />
    public virtual IDiscoveryOptionsBuilder AddTargetDatabaseEngine<TTargetEngine>() where TTargetEngine : DatabaseEngineModelAttribute, new()
    {
        if (!_databaseEngines.Add(typeof(TTargetEngine)))
        {
            throw new InvalidOperationException($"The database engine '{typeof(TTargetEngine).Name}' has already been added.");
        }
        return this;
    }

    /// <summary>
    /// Builds the <see cref="DiscoveryOptions"/> instance.
    /// </summary>
    public DiscoveryOptions Build() => new([.. _assemblies], [.. _databaseEngines]);
}
