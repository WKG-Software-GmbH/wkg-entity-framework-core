namespace Wkg.EntityFrameworkCore.Configuration.Policies.Builder;

internal class EntityPolicyComposite(List<IEntityPolicy> policies) : IEntityPolicyComponent
{
    public void AddPolicy(IEntityPolicy policy) => policies.Add(policy);

    List<IEntityPolicy> IEntityPolicyComponent.AddToAggregation(List<IEntityPolicy> aggregation)
    {
        aggregation.AddRange(policies);
        return aggregation;
    }
}
