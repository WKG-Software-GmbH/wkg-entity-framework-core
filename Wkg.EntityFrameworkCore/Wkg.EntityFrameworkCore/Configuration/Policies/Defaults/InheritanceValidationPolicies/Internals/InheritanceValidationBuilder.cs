using System.Collections.Frozen;

namespace Wkg.EntityFrameworkCore.Configuration.Policies.Defaults.InheritanceValidationPolicies.Internals;

internal class InheritanceValidationBuilder<TBaseType>(ValidationFailureDelegate failureDelegate) : IInheritanceValidationBuilder<TBaseType> where TBaseType : class
{
    private readonly HashSet<Type> _exceptedTypes = [];
    private readonly HashSet<Type> _exceptedBaseTypes = [];

    public IInheritanceValidationBuilder<TBaseType> Unless<TNonDerivedType>()
    {
        if (!_exceptedTypes.Add(typeof(TNonDerivedType)))
        {
            ThrowDuplicateException(typeof(TNonDerivedType));
        }
        return this;
    }

    public IInheritanceValidationBuilder<TBaseType> UnlessExtends<TOtherBaseType>()
    {
        if (!_exceptedBaseTypes.Add(typeof(TOtherBaseType)))
        {
            ThrowDuplicateException(typeof(TOtherBaseType));
        }
        return this;
    }

    InheritanceValidationOptions IInheritanceValidationBuilder.Build() => new(typeof(TBaseType), _exceptedTypes.ToFrozenSet(), [.. _exceptedBaseTypes], failureDelegate);

    private static void ThrowDuplicateException(Type type) => throw new InvalidOperationException($"The type '{type.Name}' was already added to the exception list.");
}