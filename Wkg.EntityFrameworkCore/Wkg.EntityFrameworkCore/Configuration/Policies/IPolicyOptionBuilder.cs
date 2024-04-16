using Wkg.Logging;

namespace Wkg.EntityFrameworkCore.Configuration.Policies;

/// <summary>
/// Represents a builder for configuring global entity validation policies.
/// </summary>
public interface IPolicyOptionBuilder
{
    /// <summary>
    /// Adds a policy to the configuration.
    /// </summary>
    /// <typeparam name="TPolicyBuilder">The type of the policy builder.</typeparam>
    /// <param name="configurePolicy">The configuration action for the policy.</param>
    /// <returns>The same <see cref="IPolicyOptionBuilder"/> instance for method chaining.</returns>
    IPolicyOptionBuilder AddPolicy<TPolicyBuilder>(Action<TPolicyBuilder> configurePolicy) where TPolicyBuilder : class, IEntityPolicyBuilder<TPolicyBuilder>;

    /// <summary>
    /// Adds a policy to the configuration.
    /// </summary>
    /// <typeparam name="TPolicyBuilder">The type of the policy builder.</typeparam>
    /// <returns>The same <see cref="IPolicyOptionBuilder"/> instance for method chaining.</returns>
    IPolicyOptionBuilder AddPolicy<TPolicyBuilder>() where TPolicyBuilder : class, IEntityPolicyBuilder<TPolicyBuilder>;

    internal bool Contains<TPolicyBuilder>() where TPolicyBuilder : class, IEntityPolicyBuilder<TPolicyBuilder>;

    internal IEntityPolicy[] Build();
}

internal class PolicyOptionBuilder : IPolicyOptionBuilder
{
    /// <summary>
    /// A lookup of policies by builder type.
    /// </summary>
    private readonly Dictionary<Type, IEntityPolicyComponent> _policies = [];

    public IPolicyOptionBuilder AddPolicy<TPolicyBuilder>(Action<TPolicyBuilder> configurePolicy) where TPolicyBuilder : class, IEntityPolicyBuilder<TPolicyBuilder> =>
        AddPolicyCore(configurePolicy);

    public IPolicyOptionBuilder AddPolicy<TPolicyBuilder>() where TPolicyBuilder : class, IEntityPolicyBuilder<TPolicyBuilder> =>
        AddPolicyCore<TPolicyBuilder>(null);

    public IPolicyOptionBuilder AddPolicyCore<TPolicyBuilder>(Action<TPolicyBuilder>? configurePolicy) where TPolicyBuilder : class, IEntityPolicyBuilder<TPolicyBuilder>
    {
        TPolicyBuilder builder = TPolicyBuilder.Create();
        configurePolicy?.Invoke(builder);
        IEntityPolicy? policy = builder.Build();
        if (policy is null)
        {
            Log.WriteWarning($"The policy of type '{typeof(TPolicyBuilder).Name}' was not added because it was left unconfigured and no default options are available for this policy type.");
            return this;
        }
        if (_policies.TryGetValue(typeof(TPolicyBuilder), out IEntityPolicyComponent? existingPolicy))
        {
            if (existingPolicy is not EntityPolicyComposite composite)
            {
                throw new InvalidOperationException($"A policy of type '{typeof(TPolicyBuilder).Name}' has already been added and cannot be combined with another policy.");
            }
            composite.AddPolicy(policy);
        }
        else
        {
            _policies.Add(typeof(TPolicyBuilder), builder.CreateComponent(policy));
        }
        return this;
    }

    bool IPolicyOptionBuilder.Contains<TPolicyBuilder>() => _policies.ContainsKey(typeof(TPolicyBuilder));

    IEntityPolicy[] IPolicyOptionBuilder.Build() =>
    [
        .. _policies.Values.Aggregate(new List<IEntityPolicy>(), (aggregate, policy) => policy.AddToAggregation(aggregate))
    ];
}

/// <summary>
/// Composite pattern for <see cref="PolicyOptionBuilder"/>.
/// </summary>
internal interface IEntityPolicyComponent
{
    List<IEntityPolicy> AddToAggregation(List<IEntityPolicy> aggregation);
}

file class EntityPolicyLeaf(IEntityPolicy policy) : IEntityPolicyComponent
{
    List<IEntityPolicy> IEntityPolicyComponent.AddToAggregation(List<IEntityPolicy> aggregation)
    {
        aggregation.Add(policy);
        return aggregation;
    }
}

file class EntityPolicyComposite(List<IEntityPolicy> policies) : IEntityPolicyComponent
{
    public void AddPolicy(IEntityPolicy policy) => policies.Add(policy);

    List<IEntityPolicy> IEntityPolicyComponent.AddToAggregation(List<IEntityPolicy> aggregation)
    {
        aggregation.AddRange(policies);
        return aggregation;
    }
}

file static class EntityPolicyBuilderExtensions
{
    public static IEntityPolicyComponent CreateComponent<TBuilder>(this TBuilder _, IEntityPolicy policy) where TBuilder : class, IEntityPolicyBuilder => 
        TBuilder.AllowMultiple
            ? new EntityPolicyComposite([policy])
            : new EntityPolicyLeaf(policy);
}