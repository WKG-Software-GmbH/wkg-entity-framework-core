﻿using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;
using System.Reflection;
using Wkg.Extensions.Reflection;
using Wkg.EntityFrameworkCore.Configuration.Reflection.Policies.NamingPolicies;
using Wkg.EntityFrameworkCore.Configuration.Reflection.Policies.MappingPolicies;

namespace Wkg.EntityFrameworkCore.Configuration.Reflection;

/// <summary>
/// Loads and configures all <see cref="IReflectiveModelConfiguration{T}"/> implementations.
/// </summary>
internal class ReflectiveModelLoader : ReflectiveLoaderBase
{
    private static object? _reflectiveModelLoaderSentinel = new();
    private static readonly HashSet<Type> _loadedDatabaseEngines = new();

    /// <summary>
    /// Loads and configures all <see cref="IReflectiveModelConfiguration{T}"/> implementations.
    /// </summary>
    /// <param name="builder">The <see cref="ModelBuilder"/> to configure.</param>
    /// <param name="databaseEngineAttributeType">The type of the attribute that marks a model as being for a specific database engine. If <see langword="null"/>, all models will be loaded.</param>
    /// <param name="namingPolicy">The policy to enforce specifying how parameter names are mapped to column names.</param>
    /// <param name="mappingPolicy">The policy to enforce specifying how to handle properties that are not explicitly mapped.</param>
    public static IReadOnlyDictionary<Type, EntityTypeBuilder> LoadAll(ModelBuilder builder, Type? databaseEngineAttributeType, INamingPolicy namingPolicy, IMappingPolicy mappingPolicy)
    {
        if (databaseEngineAttributeType is null)
        {
            AssertLoadOnce(builder, ref _reflectiveModelLoaderSentinel);
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
                Console.WriteLine($"Loading only models with {databaseEngineAttributeType.Name}.");
            }
        }
        Console.WriteLine($"{nameof(ReflectiveModelLoader)} is initializing.");
        Dictionary<Type, EntityTypeBuilder> typeBuilderCache = new();

        IEnumerable<ReflectiveEntity> entities = AssembliesWithEntryPoint()
            // get all types in these assemblies
            .SelectMany(asm => asm.GetTypes()
                .Where(type =>
                    // only keep classes
                    type.IsClass
                    // only keep classes that implement IReflectiveModelConfiguration<T> where T is that exact class
                    && type.ImplementsDirectGenericInterfaceWithTypeParameter(typeof(IReflectiveModelConfiguration<>), type)
                    // only keep classes that have the specified database engine attribute if enabled
                    && (databaseEngineAttributeType is null || type.GetCustomAttribute(databaseEngineAttributeType) is not null)))
            // just to be sure ...
            .Distinct()
            .Select(type => new ReflectiveEntity
            (
                Type: type,
                // get the exact Configure method declared by IModelConfiguration<T>
                Configure: type.GetMethod
                (
                    nameof(ModelConfigInfoForReflection_DontChange.Configure),
                    BindingFlags.Static | BindingFlags.Public | BindingFlags.DeclaredOnly,
                    new Type[] { typeof(EntityTypeBuilder<>).MakeGenericType(type) })
                ))
            .Where(entity => entity.Configure is not null)
            .ToArray();

        // re-use the same array for all calls to Configure
        object[] parameters = new object[1];
        foreach (ReflectiveEntity entity in entities)
        {
            Console.WriteLine($"{nameof(ReflectiveModelLoader)} loading: {entity.Type.Name}.");
            // get the generic Entity method
            MethodInfo? entityTypeBuilderFactory = typeof(ModelBuilder).GetMethod(nameof(ModelBuilder.Entity), 1, Array.Empty<Type>());
            // make it generic
            MethodInfo genericEntityTypeBuilderFactory = entityTypeBuilderFactory!.MakeGenericMethod(entity.Type);
            // invoke it to create an EntityTypeBuilder<T> where T matches the entity
            object entityTypeBuilderObj = genericEntityTypeBuilderFactory.Invoke(builder, null)!;
            parameters[0] = entityTypeBuilderObj!;
            // invoke the Configure method with the EntityTypeBuilder<T> instance
            entity.Configure!.Invoke(null, parameters);
            // check if this entity inherits a parent class that implements IReflectiveBaseModelConfiguration
            Type? baseType = entity.Type.BaseType;
            while (baseType is not null)
            {
                // recurse up the inheritance tree and look for any base class that implements IReflectiveBaseModelConfiguration<T> where T is the base class
                if (baseType.ImplementsDirectGenericInterfaceWithTypeParameter(typeof(IReflectiveBaseModelConfiguration<>), baseType))
                {
                    Console.WriteLine($"{nameof(ReflectiveModelLoader)} found base model: {baseType.Name}.");
                    // load the base model using the explicit interface implementation
                    // we have to do some trickery to get the correct method as it's name is compiler generated.
                    // it would be better to do this using the method table / InterfaceMapping but that just dies with some IL format error.
                    string methodName = string.Format(BaseModelConfigInfoForReflection_DontChange.RuntimeMethodName, baseType.FullName);
                    // we can't filter by arguments as the generic type is not known yet
                    MethodInfo? baseConfigure = baseType.GetMethod(methodName, BindingFlags.Static | BindingFlags.NonPublic);
                    if (baseConfigure is not null)
                    {
                        // we have the correct method, make it generic to match the child class
                        MethodInfo genericBaseConfigure = baseConfigure.MakeGenericMethod(entity.Type);
                        // invoke it with the EntityTypeBuilder<T> instance where T is the child class.
                        genericBaseConfigure.Invoke(null, parameters);
                        Console.WriteLine($"{nameof(ReflectiveModelLoader)} applied base model definition {baseType.Name} to {entity.Type.Name}.");
                    }
                }
                baseType = baseType.BaseType;
            }
            // enforce policies
            EntityTypeBuilder entityTypeBuilder = (EntityTypeBuilder)entityTypeBuilderObj;
            mappingPolicy.Audit(entityTypeBuilder.Metadata);
            namingPolicy.Audit(entityTypeBuilder.Metadata);
            typeBuilderCache.Add(entity.Type, entityTypeBuilder);
            Console.WriteLine($"{nameof(ReflectiveModelLoader)} loaded: {entity.Type.Name}.");
        }
        Console.WriteLine($"{nameof(ReflectiveModelLoader)} is exiting.");
        return typeBuilderCache;
    }
}

file class ModelConfigInfoForReflection_DontChange : IReflectiveModelConfiguration<ModelConfigInfoForReflection_DontChange>
{
    public static void Configure(EntityTypeBuilder<ModelConfigInfoForReflection_DontChange> _) => throw new NotImplementedException();
}

file class BaseModelConfigInfoForReflection_DontChange : IReflectiveBaseModelConfiguration<BaseModelConfigInfoForReflection_DontChange>
{
    public const string MethodName = nameof(IReflectiveBaseModelConfiguration<BaseModelConfigInfoForReflection_DontChange>.ConfigureBaseModel);
    public const string InterfaceName = nameof(IReflectiveBaseModelConfiguration<BaseModelConfigInfoForReflection_DontChange>);
    public static readonly string RuntimeMethodName = $"{typeof(IReflectiveBaseModelConfiguration<>).Namespace}.{InterfaceName}<{{0}}>.{MethodName}";
    static void IReflectiveBaseModelConfiguration<BaseModelConfigInfoForReflection_DontChange>.ConfigureBaseModel<TChildClass>(EntityTypeBuilder<TChildClass> self) => 
        throw new NotImplementedException();
}