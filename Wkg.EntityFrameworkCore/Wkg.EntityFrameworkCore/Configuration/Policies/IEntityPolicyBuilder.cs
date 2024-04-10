namespace Wkg.EntityFrameworkCore.Configuration.Policies;

/// <summary>
/// Represents a builder for configuring entity policies.
/// </summary>
public interface IEntityPolicyBuilder
{
    /// <summary>
    /// Builds the entity policy.
    /// </summary>
    /// <returns>A new instance of the configured entity policy, or <see langword="null"/> if the policy could not be built and no default values are provided.</returns>
    internal protected IEntityPolicy? Build();

    /// <summary>
    /// Indicates whether multiple instances of the policy can be applied, or if policies are mutually exclusive.
    /// </summary>
    internal protected static abstract bool AllowMultiple { get; }
}

/// <inheritdoc cref="IEntityPolicyBuilder"/>
/// <typeparam name="TSelf">The concrete type of the entity policy builder.</typeparam>
public interface IEntityPolicyBuilder<TSelf> : IEntityPolicyBuilder where TSelf : IEntityPolicyBuilder<TSelf>
{
    /// <summary>
    /// Creates a new instance of <typeparamref name="TSelf"/>.
    /// </summary>
    internal protected static abstract TSelf Create();
}