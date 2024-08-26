using Wkg.EntityFrameworkCore.Configuration.Policies.Defaults.InheritanceValidationPolicies.Internals;

namespace Wkg.EntityFrameworkCore.Configuration.Policies.Defaults.InheritanceValidationPolicies;

/// <summary>
/// Configures the inheritance validation policy for entities, allowing for the enforcement of strict inheritance rules with support for certain exceptions.
/// </summary>
public interface IInheritanceValidationBuilder
{
    internal InheritanceValidationOptions Build();
}

/// <inheritdoc cref="IInheritanceValidationBuilder"/>
/// <typeparam name="TBaseType">The base type that all entities must extend.</typeparam>
public interface IInheritanceValidationBuilder<TBaseType> : IInheritanceValidationBuilder where TBaseType : class
{
    /// <summary>
    /// Requires that all entities extend a specific base type, unless they are of concrete type <typeparamref name="TNonDerivedType"/>, in which case the inheritance rule is not enforced.
    /// </summary>
    /// <typeparam name="TNonDerivedType">The type that is exempt from the inheritance rule.</typeparam>
    /// <returns>This instance for method chaining.</returns>
    IInheritanceValidationBuilder<TBaseType> Unless<TNonDerivedType>();

    /// <summary>
    /// Requires that all entities extend a specific base type, unless they extend a different base type <typeparamref name="TOtherBaseType"/>, in which case the inheritance rule is not enforced.
    /// </summary>
    /// <typeparam name="TOtherBaseType">The other base type whose inheritance is exempt from the rule.</typeparam>
    /// <returns>This instance for method chaining.</returns>
    IInheritanceValidationBuilder<TBaseType> UnlessExtends<TOtherBaseType>();
}
