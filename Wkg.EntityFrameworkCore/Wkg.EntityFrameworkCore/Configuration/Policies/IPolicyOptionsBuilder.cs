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

    /// <summary>
    /// Determines whether the configuration contains a policy of the specified type.
    /// </summary>
    /// <typeparam name="TPolicyBuilder">The type of the policy builder.</typeparam>
    /// <returns><see langword="true"/> if the configuration contains a policy of the specified type; otherwise, <see langword="false"/>.</returns>
    bool Contains<TPolicyBuilder>() where TPolicyBuilder : class, IEntityPolicyBuilder<TPolicyBuilder>;

    /// <summary>
    /// Builds the entity policies.
    /// </summary>
    IEntityPolicy[] Build();
}
