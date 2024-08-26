namespace Wkg.EntityFrameworkCore.Configuration.Policies;

/// <summary>
/// Represents a builder for configuring global entity validation policies.
/// </summary>
public interface IPolicyOptionsBuilder
{
    /// <summary>
    /// Adds a policy to the configuration.
    /// </summary>
    /// <typeparam name="TPolicyBuilder">The type of the policy builder.</typeparam>
    /// <param name="configurePolicy">The configuration action for the policy.</param>
    /// <returns>The same <see cref="IPolicyOptionsBuilder"/> instance for method chaining.</returns>
    IPolicyOptionsBuilder AddPolicy<TPolicyBuilder>(Action<TPolicyBuilder> configurePolicy) where TPolicyBuilder : class, IEntityPolicyBuilder<TPolicyBuilder>;

    /// <summary>
    /// Adds a policy to the configuration.
    /// </summary>
    /// <typeparam name="TPolicyBuilder">The type of the policy builder.</typeparam>
    /// <returns>The same <see cref="IPolicyOptionsBuilder"/> instance for method chaining.</returns>
    IPolicyOptionsBuilder AddPolicy<TPolicyBuilder>() where TPolicyBuilder : class, IEntityPolicyBuilder<TPolicyBuilder>;

    internal bool Contains<TPolicyBuilder>() where TPolicyBuilder : class, IEntityPolicyBuilder<TPolicyBuilder>;

    internal IEntityPolicy[] Build();
}
