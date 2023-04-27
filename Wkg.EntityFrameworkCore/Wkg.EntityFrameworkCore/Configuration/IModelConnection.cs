using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Wkg.EntityFrameworkCore.Configuration;

public interface IModelConnection<TConnection, TLeft, TRight> 
    where TConnection : class, IModelConnection<TConnection, TLeft, TRight>
    where TLeft : class, IModelConfiguration<TLeft>
    where TRight : class, IModelConfiguration<TRight>
{
    static abstract void Connect(EntityTypeBuilder<TLeft> left, EntityTypeBuilder<TRight> right);

    static abstract void ConfigureConnection(EntityTypeBuilder<TConnection> self);
}
