using Wkg.EntityFrameworkCore.Configuration.Policies;
using Wkg.EntityFrameworkCore.Configuration.Reflection.Attributes;

namespace Wkg.EntityFrameworkCore.Configuration.Reflection.Discovery;

/// <summary>
/// Represents a builder for configuring global entity discovery options.
/// </summary>
public interface IDiscoveryOptionsBuilder
{
    /// <summary>
    /// Specifies an assembly to be included in entity discovery.
    /// </summary>
    /// <typeparam name="TTargetAssembly">The type implementing <see cref="ITargetAssembly"/> which represents the assembly to be included in entity discovery.</typeparam>
    /// <returns>This instance for method chaining.</returns>
    IDiscoveryOptionsBuilder AddTargetAssembly<TTargetAssembly>() where TTargetAssembly : class, ITargetAssembly;

    /// <summary>
    /// Specifies a database engine-specific attribute that should be included in entity discovery. 
    /// If no database engines are specified, all entities are included in discovery.
    /// </summary>
    /// <typeparam name="TTargetEngine">The type of the database engine attribute to be included in entity discovery, which must be a subclass of <see cref="DatabaseEngineModelAttribute"/>.</typeparam>
    /// <returns>This instance for method chaining.</returns>
    IDiscoveryOptionsBuilder AddTargetDatabaseEngine<TTargetEngine>() where TTargetEngine : DatabaseEngineModelAttribute, new();
}