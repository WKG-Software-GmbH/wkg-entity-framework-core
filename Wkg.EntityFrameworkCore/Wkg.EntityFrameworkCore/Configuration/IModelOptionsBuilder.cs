using Wkg.EntityFrameworkCore.Configuration.Policies;
using Wkg.EntityFrameworkCore.Configuration.Reflection.Discovery;

namespace Wkg.EntityFrameworkCore.Configuration;

/// <summary>
/// Represents a builder for configuring global model options.
/// </summary>
public interface IModelOptionsBuilder
{
    /// <summary>
    /// Configures global entity discovery options.
    /// </summary>
    /// <param name="configure">The action to configure the discovery options.</param>
    /// <returns>The same <see cref="IModelOptionsBuilder"/> instance for method chaining.</returns>
    IModelOptionsBuilder ConfigureDiscovery(Action<IDiscoveryOptionsBuilder> configure);

    /// <summary>
    /// Configures global entity validation policies.
    /// </summary>
    /// <param name="configure">The action to configure the policy options.</param>
    /// <returns>The same <see cref="IModelOptionsBuilder"/> instance for method chaining.</returns>
    IModelOptionsBuilder ConfigurePolicies(Action<IPolicyOptionsBuilder> configure);
}
