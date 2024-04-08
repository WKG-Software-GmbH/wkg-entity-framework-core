namespace Wkg.EntityFrameworkCore.Configuration.Policies;

public interface IEntityPolicyBuilder
{
    internal protected IEntityPolicy? Build();
    internal protected static abstract bool AllowMultiple { get; }
}

public interface IEntityPolicyBuilder<TSelf> : IEntityPolicyBuilder where TSelf : IEntityPolicyBuilder<TSelf>
{
    internal protected static abstract TSelf Create();
}