﻿using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Wkg.EntityFrameworkCore.Configuration;

/// <summary>
/// Represents a many to many relationship between two entities.
/// </summary>
/// <typeparam name="TConnection">The type of the implementing connection entity.</typeparam>
/// <typeparam name="TLeft">The type of the left entity.</typeparam>
/// <typeparam name="TRight">The type of the right entity.</typeparam>
public interface IModelConnection<TConnection, TLeft, TRight> 
    where TConnection : class, IModelConnection<TConnection, TLeft, TRight>
    where TLeft : class, IModelConfiguration<TLeft>
    where TRight : class, IModelConfiguration<TRight>
{
    /// <summary>
    /// Configures the many to many relationship between the two entities.
    /// </summary>
    /// <param name="left">The left entity.</param>
    /// <param name="right">The right entity.</param>
    static abstract void Connect(EntityTypeBuilder<TLeft> left, EntityTypeBuilder<TRight> right);

    /// <summary>
    /// Configures the connection entity.
    /// </summary>
    /// <remarks>
    /// This method should be passed to <see cref="CollectionCollectionBuilder{TLeftEntity, TRightEntity}.UsingEntity{TJoinEntity}(Func{EntityTypeBuilder{TJoinEntity}, ReferenceCollectionBuilder{TLeftEntity, TJoinEntity}}, Func{EntityTypeBuilder{TJoinEntity}, ReferenceCollectionBuilder{TRightEntity, TJoinEntity}}, Action{EntityTypeBuilder{TJoinEntity}})"/> to configure the connection entity in the <see cref="Connect(EntityTypeBuilder{TLeft}, EntityTypeBuilder{TRight})"/> method.
    /// </remarks>
    /// <param name="self">The connection entity.</param>
    static abstract void ConfigureConnection(EntityTypeBuilder<TConnection> self);
}
