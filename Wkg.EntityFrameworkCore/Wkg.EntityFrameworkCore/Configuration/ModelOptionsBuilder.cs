using Wkg.EntityFrameworkCore.Configuration.Policies;
using Wkg.EntityFrameworkCore.Configuration.Policies.Builder;
using Wkg.EntityFrameworkCore.Configuration.Reflection.Discovery;

namespace Wkg.EntityFrameworkCore.Configuration;

internal class ModelOptionsBuilder : IModelOptionsBuilder
{
    private bool _discoveryOptionsConfigured;
    private bool _policyOptionsConfigured;

    public DiscoveryOptionsBuilder DiscoveryOptionsBuilder { get; } = new();

    public IPolicyOptionsBuilder PolicyOptionsBuilder { get; } = new PolicyOptionsBuilder();

    public IModelOptionsBuilder ConfigureDiscovery(Action<IDiscoveryOptionsBuilder> configure)
    {
        if (_discoveryOptionsConfigured)
        {
            throw new InvalidOperationException("Discovery options have already been configured.");
        }
        configure(DiscoveryOptionsBuilder);
        _discoveryOptionsConfigured = true;
        return this;
    }

    public IModelOptionsBuilder ConfigurePolicies(Action<IPolicyOptionsBuilder> configure)
    {
        if (_policyOptionsConfigured)
        {
            throw new InvalidOperationException("Policy options have already been configured.");
        }
        configure(PolicyOptionsBuilder);
        _policyOptionsConfigured = true;
        return this;
    }
}