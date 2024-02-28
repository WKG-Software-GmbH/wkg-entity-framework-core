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
    DiscoveryOptionsBuilder AddDiscoveryTarget<TTargetAssembly>() where TTargetAssembly : class, ITargetAssembly;
}