using Wkg.Logging;

namespace Wkg.EntityFrameworkCore.Configuration.Policies.Builder;

internal class PolicyOptionsBuilder : IPolicyOptionsBuilder
{
    /// <summary>
    /// A lookup of policies by builder type.
    /// </summary>
    private readonly Dictionary<Type, IEntityPolicyComponent> _policies = [];

    public IPolicyOptionsBuilder AddPolicy<TPolicyBuilder>(Action<TPolicyBuilder> configurePolicy) where TPolicyBuilder : class, IEntityPolicyBuilder<TPolicyBuilder> =>
        AddPolicyCore(configurePolicy);

    public IPolicyOptionsBuilder AddPolicy<TPolicyBuilder>() where TPolicyBuilder : class, IEntityPolicyBuilder<TPolicyBuilder> =>
        AddPolicyCore<TPolicyBuilder>(null);

    public IPolicyOptionsBuilder AddPolicyCore<TPolicyBuilder>(Action<TPolicyBuilder>? configurePolicy) where TPolicyBuilder : class, IEntityPolicyBuilder<TPolicyBuilder>
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

    bool IPolicyOptionsBuilder.Contains<TPolicyBuilder>() => _policies.ContainsKey(typeof(TPolicyBuilder));

    IEntityPolicy[] IPolicyOptionsBuilder.Build() =>
    [
        .. _policies.Values.Aggregate(new List<IEntityPolicy>(), (aggregate, policy) => policy.AddToAggregation(aggregate))
    ];
}
