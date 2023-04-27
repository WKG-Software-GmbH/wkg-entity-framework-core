using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Wkg.EntityFrameworkCore.Configuration;

public interface IBaseModelConfiguration<TChildClass> where TChildClass : class, IModelConfiguration<TChildClass>
{
    static abstract void ConfigureBaseModel(EntityTypeBuilder<TChildClass> self);
}