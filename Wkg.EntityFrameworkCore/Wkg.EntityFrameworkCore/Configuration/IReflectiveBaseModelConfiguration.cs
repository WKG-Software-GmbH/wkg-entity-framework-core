using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Wkg.EntityFrameworkCore.Configuration;

public interface IReflectiveBaseModelConfiguration<TParentClass> where TParentClass : class, IReflectiveBaseModelConfiguration<TParentClass>
{
    protected static abstract void ConfigureBaseModel<TChildClass>(EntityTypeBuilder<TChildClass> self) 
        where TChildClass : class, TParentClass, IModelConfiguration<TChildClass>;
}