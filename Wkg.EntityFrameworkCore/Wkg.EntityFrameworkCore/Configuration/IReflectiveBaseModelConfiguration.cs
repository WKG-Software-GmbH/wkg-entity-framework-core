using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Wkg.EntityFrameworkCore.Configuration;

/// <summary>
/// Represents a reflectively-loaded configuration for an abstract base model when using Table-Per-Concrete-Type (TPC) inheritance.
/// </summary>
/// <typeparam name="TParentClass">The type of the parent class.</typeparam>
public interface IReflectiveBaseModelConfiguration<TParentClass> where TParentClass : class, IReflectiveBaseModelConfiguration<TParentClass>
{
    /// <summary>
    /// Configures shared properties of the base model.
    /// </summary>
    /// <typeparam name="TChildClass">The type of the child class.</typeparam>
    /// <param name="self">The entity type builder of the child class.</param>
    protected static abstract void ConfigureBaseModel<TChildClass>(EntityTypeBuilder<TChildClass> self) 
        where TChildClass : class, TParentClass, IModelConfiguration<TChildClass>;
}