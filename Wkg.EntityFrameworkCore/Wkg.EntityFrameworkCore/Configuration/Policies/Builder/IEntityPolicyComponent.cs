namespace Wkg.EntityFrameworkCore.Configuration.Policies.Builder;

/// <summary>
/// Composite pattern for <see cref="PolicyOptionsBuilder"/>.
/// </summary>
internal interface IEntityPolicyComponent
{
    List<IEntityPolicy> AddToAggregation(List<IEntityPolicy> aggregation);
}
