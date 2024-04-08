namespace Wkg.EntityFrameworkCore.Configuration.Policies.ColumnNamingPolicies;

public static class PolicyOptionBuilderExtensions
{
    public static IPolicyOptionBuilder AddColumnNamingPolicy(this IPolicyOptionBuilder builder, ColumnNamingPolicy namingPolicy) => 
        builder.AddPolicy<ColumnNaming>(policy => _ = namingPolicy switch
        {
            ColumnNamingPolicy.AllowImplicit => Do(policy.AllowImplicit),
            ColumnNamingPolicy.PreferExplicit => Do(policy.PreferExplicit),
            ColumnNamingPolicy.RequireExplicit => Do(policy.RequireExplicit),
            _ => throw new ArgumentOutOfRangeException(nameof(namingPolicy), namingPolicy, "Invalid naming policy.")
        });
}

public enum ColumnNamingPolicy
{
    AllowImplicit,
    PreferExplicit,
    RequireExplicit
}
