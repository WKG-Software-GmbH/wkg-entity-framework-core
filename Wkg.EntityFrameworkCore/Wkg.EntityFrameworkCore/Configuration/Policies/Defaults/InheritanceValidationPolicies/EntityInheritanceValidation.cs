using Wkg.EntityFrameworkCore.Configuration.Policies.Defaults.InheritanceValidationPolicies.Internals;
using Wkg.Logging;
using Wkg.Logging.Writers;

namespace Wkg.EntityFrameworkCore.Configuration.Policies.Defaults.InheritanceValidationPolicies;

/// <summary>
/// Configures the inheritance validation policy for entities, allowing for the enforcement of strict inheritance rules.
/// </summary>
public class EntityInheritanceValidation : IEntityPolicyBuilder<EntityInheritanceValidation>
{
    private IInheritanceValidationBuilder? _inner;

    static bool IEntityPolicyBuilder.AllowMultiple => true;

    static EntityInheritanceValidation IEntityPolicyBuilder<EntityInheritanceValidation>.Create() => new();

    /// <summary>
    /// Requires that all entities extend a specific base type. If an entity does not extend the specified base type, an exception of type <see cref="PolicyViolationException"/> is thrown.
    /// </summary>
    /// <typeparam name="TBaseType">The base type that all entities must extend.</typeparam>
    /// <returns>A builder for configuring the inheritance validation policy for the specified base type.</returns>
    public IInheritanceValidationBuilder<TBaseType> MustExtend<TBaseType>() where TBaseType : class
    {
        InheritanceValidationBuilder<TBaseType> builder = new(error => throw new PolicyViolationException(error));
        _inner = builder;
        return builder;
    }

    /// <summary>
    /// Indicates that all entities should extend a specific base type. If an entity does not extend the specified base type, a warning is logged.
    /// </summary>
    /// <typeparam name="TBaseType">The base type that all entities should extend.</typeparam>
    /// <returns>A builder for configuring the inheritance validation policy for the specified base type.</returns>
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