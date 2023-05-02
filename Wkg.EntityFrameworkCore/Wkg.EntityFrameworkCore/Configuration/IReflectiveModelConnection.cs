﻿using Microsoft.EntityFrameworkCore;
using Wkg.EntityFrameworkCore.Configuration.Reflection.Policies.MappingPolicies;
using Wkg.EntityFrameworkCore.Configuration.Reflection.Policies.NamingPolicies;
using Wkg.EntityFrameworkCore.Extensions;

namespace Wkg.EntityFrameworkCore.Configuration;

/// <summary>
/// Represents a many to many connection between two entities that will be reflectively configured by the <see cref="ModelBuilderExtensions.LoadReflectiveModels(ModelBuilder, INamingPolicy, IMappingPolicy)"/> method.
/// </summary>
/// <typeparam name="TConnection">The type of the implementing connection entity.</typeparam>
/// <typeparam name="TLeft">The type of the left entity.</typeparam>
/// <typeparam name="TRight">The type of the right entity.</typeparam>
public interface IReflectiveModelConnection<TConnection, TLeft, TRight> : IModelConnection<TConnection, TLeft, TRight>
    where TConnection : class, IReflectiveModelConnection<TConnection, TLeft, TRight>
    where TLeft : class, IReflectiveModelConfiguration<TLeft>
    where TRight : class, IReflectiveModelConfiguration<TRight>
{
}
