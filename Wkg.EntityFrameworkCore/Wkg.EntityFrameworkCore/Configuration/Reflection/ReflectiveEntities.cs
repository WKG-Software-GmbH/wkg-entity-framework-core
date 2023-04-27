using System.Reflection;

namespace Wkg.EntityFrameworkCore.Configuration.Reflection;

/// <summary>
/// Represents an entity that can be configured using reflection.
/// </summary>
/// <param name="Type">The type of the entity.</param>
/// <param name="Configure">The method to configure the entity.</param>
public readonly record struct ReflectiveEntity(Type Type, MethodInfo? Configure);

/// <summary>
/// Represents a procedure that can be configured using reflection.
/// </summary>
/// <param name="ProcedureType">The type of the procedure.</param>
/// <param name="ContextType">The type of the context acting as the parameter object of the procedure.</param>
public readonly record struct ReflectiveProcedure(Type ProcedureType, Type? ContextType);

/// <summary>
/// Represents a connection that can be configured using reflection.
/// </summary>
/// <param name="Type">The type of the connection.</param>
/// <param name="TFrom">The type of the source entity.</param>
/// <param name="TTo">The type of the target entity.</param>
/// <param name="Connect">The method to configure the connection.</param>
public readonly record struct ReflectiveConnection(Type Type, Type TFrom, Type TTo, MethodInfo? Connect);