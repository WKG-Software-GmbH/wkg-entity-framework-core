﻿using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;
using System.Reflection;
using Wkg.Extensions.Reflection;
using Wkg.Logging;
using Wkg.EntityFrameworkCore.Configuration.Policies.Discovery;

namespace Wkg.EntityFrameworkCore.Configuration.Reflection;

/// <summary>
/// Loads and configures all <see cref="IReflectiveModelConnection{TConnection, TSource, TTarget}"/> implementations.
/// </summary>
internal class ReflectiveConnectionLoader : ReflectiveLoaderBase
{
    private static object? _reflectiveConnectionLoaderSentinel = new();
    private static readonly HashSet<Type> _loadedDatabaseEngines = new();

    /// <summary>
    /// Loads and configures all <see cref="IReflectiveModelConnection{TConnection, TSource, TTarget}"/> implementations.
    /// </summary>
    /// <param name="builder">The <see cref="ModelBuilder"/> to configure.</param>
    /// <param name="discoveryContext">The <see cref="IDiscoveryContext"/> that has been used for model discovery.</param>
    /// <param name="databaseEngineAttributeType">The type of the attribute that marks a model connection as being for a specific database engine. If <see langword="null"/>, all models will be loaded.</param>
    public static void LoadAll(ModelBuilder builder, IDiscoveryContext discoveryContext, Type? databaseEngineAttributeType)
    {
        if (databaseEngineAttributeType is null)
        {
            AssertLoadOnce(builder, ref _reflectiveConnectionLoaderSentinel);
        }
        else
        {
            lock(_loadedDatabaseEngines)
            {
                if (_loadedDatabaseEngines.Contains(databaseEngineAttributeType))
                {
                    throw new InvalidOperationException($"The database engine {databaseEngineAttributeType.Name} has already been loaded.");
                }
                _loadedDatabaseEngines.Add(databaseEngineAttributeType);
                Log.WriteInfo($"Loading only model connections with {databaseEngineAttributeType.Name}.");
            }
        }

        Log.WriteInfo($"{nameof(ReflectiveConnectionLoader)} is initializing.");

        ReflectiveConnection[] connections = AssembliesWithEntryPoint()
            // get all types in these assemblies
            .SelectMany(asm => asm.GetTypes()
                .Where(type =>
                    // only keep classes
                    type.IsClass
                    // only keep classes that implement IReflectiveModelConnection<TConnection, TFrom, TTo>
                    && type.ImplementsGenericInterfaceDirectly(typeof(IReflectiveModelConnection<,,>))
                    // only keep classes that have the specified database engine attribute if enabled
                    && (databaseEngineAttributeType is null || type.GetCustomAttribute(databaseEngineAttributeType) is not null)))
            // just to be sure ...
            .Distinct()
            .Select(type => 
            (
                Type: type, 
                TypeArgs: type.GetGenericTypeArgumentsOfSingleDirectInterface(typeof(IReflectiveModelConnection<,,>))
            ))
            .Where(t => t.TypeArgs is { Length: 3 } 
                // TConnection must match the implementing type
                && t.TypeArgs[0] == t.Type
                // TFrom and TTo must implement IReflectiveModelConfiguration<T> (be reflectively loaded)
                && t.TypeArgs.Skip(1).All(typeParam => typeParam.ImplementsDirectGenericInterfaceWithTypeParameter(typeof(IReflectiveModelConfiguration<>), typeParam)))
            .Select(type => new ReflectiveConnection
            (
                Type: type.Type,
                TFrom: type.TypeArgs![1],
                TTo: type.TypeArgs[2],
                // get the exact Configure method declared by IModelConfiguration<T>
                Connect: type.Type.GetMethod
                (
                    nameof(ModelConnectionInfoForReflection_DontChange.Connect),
                    BindingFlags.Static | BindingFlags.Public | BindingFlags.DeclaredOnly,
                    new Type[] 
                    { 
                        typeof(EntityTypeBuilder<>).MakeGenericType(type.TypeArgs[1]), 
                        typeof(EntityTypeBuilder<>).MakeGenericType(type.TypeArgs[2]) 
                    })
                ))
            .Where(connection => connection.Connect is not null)
            .ToArray();

        Log.WriteInfo($"{nameof(ReflectiveConnectionLoader)} discovered {connections.Length} model connections.");

        // re-use the same array for all calls to Configure
        object[] parameters = new object[2];
        foreach (ReflectiveConnection connection in connections)
        {
            Log.WriteDiagnostic($"{nameof(ReflectiveConnectionLoader)} loading: {connection.Type.Name}.");
            // ensure that the entity types are configured
            if (discoveryContext.EntityBuilderCache.TryGetValue(connection.TFrom, out EntityTypeBuilder? fromBuilder) is false)
            {
                throw new InvalidOperationException($"The entity type {connection.TFrom.Name} is not configured.");
            }
            if (discoveryContext.EntityBuilderCache.TryGetValue(connection.TTo, out EntityTypeBuilder? toBuilder) is false)
            {
                throw new InvalidOperationException($"The entity type {connection.TTo.Name} is not configured.");
            }
            // if the connection implements IModelConfiguration<T>, then it would be configured twice.
            if (connection.Type.ImplementsGenericInterface(typeof(IModelConfiguration<>)))
            {
                throw new InvalidOperationException($"The model connection {connection.Type.Name} implements IModelConnection and IModelConfiguration at the same time. This is not supported. Use IModelConnection.ConfigureConnection to configure the connection entity.");
            }
            parameters[0] = fromBuilder;
            parameters[1] = toBuilder;

            // invoke the Connect method with the two EntityTypeBuilder<T> instances
            connection.Connect!.Invoke(null, parameters);
            Log.WriteDiagnostic($"{nameof(ReflectiveConnectionLoader)} loaded: {connection.Type.Name}.");

            // create an entity type builder for the connection entity and add it to the cache for policy validation
            EntityTypeBuilder connectionBuilder = builder.Entity(connection.Type);
            discoveryContext.EntityBuilderCache.Add(connection.Type, connectionBuilder);
        }
        Log.WriteInfo($"{nameof(ReflectiveConnectionLoader)} loaded {connections.Length} model connections.");
        Log.WriteInfo($"{nameof(ReflectiveConnectionLoader)} is exiting.");
    }
}

file class ModelConnectionInfoForReflection_DontChange
    : IReflectiveModelConfiguration<ModelConnectionInfoForReflection_DontChange>,
    IReflectiveModelConnection<ModelConnectionInfoForReflection_DontChange, ModelConnectionInfoForReflection_DontChange, ModelConnectionInfoForReflection_DontChange>
{
    public static void Configure(EntityTypeBuilder<ModelConnectionInfoForReflection_DontChange> _) => 
        throw new NotSupportedException();

    public static void ConfigureConnection(EntityTypeBuilder<ModelConnectionInfoForReflection_DontChange> self) => 
        throw new NotSupportedException();

    public static void Connect(EntityTypeBuilder<ModelConnectionInfoForReflection_DontChange> _, EntityTypeBuilder<ModelConnectionInfoForReflection_DontChange> _1) => 
        throw new NotSupportedException();
}