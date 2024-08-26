namespace Wkg.EntityFrameworkCore.Configuration.Policies.Builder;

internal class EntityPolicyLeaf(IEntityPolicy policy) : IEntityPolicyComponent
{
    List<IEntityPolicy> IEntityPolicyComponent.AddToAggregation(List<IEntityPolicy> aggregation)
    {
        aggregation.Add(policy);
        return aggregation;
    }
}
