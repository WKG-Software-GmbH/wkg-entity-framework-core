using System.Collections.Frozen;
using System.Collections.Immutable;

namespace Wkg.EntityFrameworkCore.Configuration.Policies.InheritanceValidationPolicies.Internals;

internal record InheritanceValidationOptions(Type RequiredBaseType, FrozenSet<Type> ExceptedTypes, ImmutableArray<Type> ExceptedBaseTypes, ValidationFailureDelegate ValidationFailureDelegate)
{
    public void OnValidationFailure(Type type) => 
        ValidationFailureDelegate($"Type '{type}' violates one of the specified inheritance validation policies: must inherit from '{RequiredBaseType}'. No exceptions were specified for this policy or type.");
}
