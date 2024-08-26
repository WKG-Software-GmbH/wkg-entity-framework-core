namespace Wkg.EntityFrameworkCore.Configuration.Policies.Builder;

internal static class EntityPolicyBuilderExtensions
{
    public static IEntityPolicyComponent CreateComponent<TBuilder>(this TBuilder _, IEntityPolicy policy) where TBuilder : class, IEntityPolicyBuilder =>
        TBuilder.AllowMultiple
            ? new EntityPolicyComposite([policy])
            : new EntityPolicyLeaf(policy);
}