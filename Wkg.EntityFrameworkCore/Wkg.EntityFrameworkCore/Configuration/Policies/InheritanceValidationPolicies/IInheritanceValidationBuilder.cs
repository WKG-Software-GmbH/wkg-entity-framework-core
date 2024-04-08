using Wkg.EntityFrameworkCore.Configuration.Policies.InheritanceValidationPolicies.Internals;

namespace Wkg.EntityFrameworkCore.Configuration.Policies.InheritanceValidationPolicies;

public interface IInheritanceValidationBuilder
{
    internal InheritanceValidationOptions Build();
}

public interface IInheritanceValidationBuilder<TBaseType> : IInheritanceValidationBuilder where TBaseType : class
{
    IInheritanceValidationBuilder<TBaseType> Unless<TNonDerivedType>();

    IInheritanceValidationBuilder<TBaseType> UnlessExtends<TOtherBaseType>();
}
