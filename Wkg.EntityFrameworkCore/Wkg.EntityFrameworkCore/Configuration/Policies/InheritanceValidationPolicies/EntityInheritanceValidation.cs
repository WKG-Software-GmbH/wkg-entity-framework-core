using Wkg.EntityFrameworkCore.Configuration.Policies.InheritanceValidationPolicies.Internals;
using Wkg.Logging;
using Wkg.Logging.Writers;

namespace Wkg.EntityFrameworkCore.Configuration.Policies.InheritanceValidationPolicies;

public class EntityInheritanceValidation : IEntityPolicyBuilder<EntityInheritanceValidation>
{
    private IInheritanceValidationBuilder? _inner;

    static bool IEntityPolicyBuilder.AllowMultiple => true;

    static EntityInheritanceValidation IEntityPolicyBuilder<EntityInheritanceValidation>.Create() => new();

    public IInheritanceValidationBuilder<TBaseType> MustExtend<TBaseType>() where TBaseType : class
    {
        InheritanceValidationBuilder<TBaseType> builder = new(error => throw new PolicyViolationException(error));
        _inner = builder;
        return builder;
    }

    public IInheritanceValidationBuilder<TBaseType> ShouldExtend<TBaseType>() where TBaseType : class
    {
        InheritanceValidationBuilder<TBaseType> builder = new(error => Log.WriteWarning(error, LogWriter.Blocking));
        _inner = builder;
        return builder;
    }

    IEntityPolicy? IEntityPolicyBuilder.Build()
    {
        if (_inner is null)
        {
            return null;
        }
        InheritanceValidationOptions options = _inner.Build();
        return new InheritanceValidationPolicy(options);
    }
}