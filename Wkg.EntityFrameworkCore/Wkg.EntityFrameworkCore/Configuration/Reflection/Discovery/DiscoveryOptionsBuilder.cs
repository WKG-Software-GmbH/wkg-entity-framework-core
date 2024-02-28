using System.Reflection;

namespace Wkg.EntityFrameworkCore.Configuration.Reflection.Discovery;

/// <summary>
/// Configures global entity discovery options.
/// </summary>
public class DiscoveryOptionsBuilder : IDiscoveryOptionsBuilder
{
    private readonly List<Assembly> _assemblies = [];

    /// <summary>
    /// Creates a new instance of <see cref="DiscoveryOptionsBuilder"/>.
    /// </summary>
    internal protected DiscoveryOptionsBuilder() => Pass();

    /// <inheritdoc />
    public DiscoveryOptionsBuilder AddDiscoveryTarget<TTargetAssembly>() where TTargetAssembly : class, ITargetAssembly
    {
        _assemblies.Add(TTargetAssembly.Assembly);
        return this;
    }

    /// <summary>
    /// Builds the <see cref="DiscoveryOptions"/> instance.
    /// </summary>
    public DiscoveryOptions Build() => new([.. _assemblies]);
}
