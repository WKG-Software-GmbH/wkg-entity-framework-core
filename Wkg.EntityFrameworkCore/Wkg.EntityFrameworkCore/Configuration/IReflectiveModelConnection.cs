namespace Wkg.EntityFrameworkCore.Configuration;

public interface IReflectiveModelConnection<TConnection, TLeft, TRight> : IModelConnection<TConnection, TLeft, TRight>
    where TConnection : class, IReflectiveModelConnection<TConnection, TLeft, TRight>
    where TLeft : class, IReflectiveModelConfiguration<TLeft>
    where TRight : class, IReflectiveModelConfiguration<TRight>
{
}
