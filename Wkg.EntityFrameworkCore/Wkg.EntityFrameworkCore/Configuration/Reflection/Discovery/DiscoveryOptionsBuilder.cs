using System.Reflection;

namespace Wkg.EntityFrameworkCore.Configuration.Reflection.Discovery;

/// <summary>
/// Configures global entity discovery options.
/// </summary>
public class DiscoveryOptionsBuilder
{
    private readonly List<Assembly> _assemblies = [];

    internal DiscoveryOptionsBuilder() => Pass();

    /// <summary>
    /// Specifies an assembly to be included in entity discovery.
    /// </summary>
    /// <typeparam name="TTargetAssembly">The type implementing <see cref="ITargetAssembly"/> which represents the assembly to be included in entity discovery.</typeparam>
    /// <returns>This instance for method chaining.</returns>
    public DiscoveryOptionsBuilder AddDiscoveryTarget<TTargetAssembly>() where TTargetAssembly : class, ITargetAssembly
    {
        _assemblies.Add(TTargetAssembly.Assembly);
        return this;
    }

    internal DiscoveryOptions Build() => new([.. _assemblies]);
}
