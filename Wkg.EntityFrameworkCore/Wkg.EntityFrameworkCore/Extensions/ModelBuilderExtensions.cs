using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Wkg.EntityFrameworkCore.Configuration;
using Wkg.EntityFrameworkCore.Configuration.Policies;
using Wkg.EntityFrameworkCore.Configuration.Policies.Discovery;
using Wkg.EntityFrameworkCore.Configuration.Policies.MappingPolicies;
using Wkg.EntityFrameworkCore.Configuration.Policies.NamingPolicies;
using Wkg.EntityFrameworkCore.Configuration.Reflection;
using Wkg.EntityFrameworkCore.Configuration.Reflection.Attributes;

namespace Wkg.EntityFrameworkCore.Extensions;

/// <summary>
/// Holds extension methods for <see cref="ModelBuilder"/>.
/// </summary>
public static class ModelBuilderExtensions
{
    /// <summary>
    /// Initializes a new <see cref="IDiscoveryContext"/> using the specified <paramref name="namingPolicy"/> and <paramref name="mappingPolicy"/>.
    /// </summary>
    /// <param name="_">The model builder.</param>
    /// <param name="namingPolicy">The <see cref="INamingPolicy"/> to use. If <see langword="null"/>, <see cref="NamingPolicy.RequireExplicit"/> will be used.</param>
    /// <param name="mappingPolicy">The <see cref="IMappingPolicy"/> to use. If <see langword="null"/>, <see cref="PropertyMappingPolicy.IgnoreImplicit"/> will be used.</param>
    /// <returns>The <see cref="IDiscoveryContext"/>.</returns>
    public static IDiscoveryContext CreateDiscoveryContext(this ModelBuilder _, INamingPolicy? namingPolicy = null, IMappingPolicy? mappingPolicy = null) => 
        new EntityDiscoveryContext(namingPolicy, mappingPolicy);

    /// <summary>
    /// Loads and configures the specified <typeparamref name="TModel"/>.
    /// </summary>
    /// <typeparam name="TModel">The type of the model.</typeparam>
    /// <param name="builder">The model builder.</param>
    /// <param name="discoveryContext">The <see cref="IDiscoveryContext"/> to be used for discovery. 
    /// The discovery context can later be used to enforce policies on the discovered entities.</param>
    /// <returns>The model builder.</returns>
    public static ModelBuilder LoadModel<TModel>(this ModelBuilder builder, IDiscoveryContext? discoveryContext = null)
        where TModel : class, IModelConfiguration<TModel>
    {
        EntityTypeBuilder<TModel> entityBuilder = builder.Entity<TModel>();
        TModel.Configure(entityBuilder);
        discoveryContext?.EntityBuilderCache.Add(typeof(TModel), entityBuilder);
        return builder;
    }

    /// <summary>
    /// Loads and configures the specified <typeparamref name="TConnection"/> entity between the specified <typeparamref name="TLeft"/> and <typeparamref name="TRight"/> entities.
    /// </summary>
    /// <typeparam name="TConnection">The type of the connection entity.</typeparam>
    /// <typeparam name="TLeft">The type of the left entity.</typeparam>
    /// <typeparam name="TRight">The type of the right entity.</typeparam>
    /// <param name="builder">The model builder.</param>
    /// <param name="discoveryContext">The <see cref="IDiscoveryContext"/> to be used for discovery. 
    /// The discovery context can later be used to enforce policies on the discovered entities.</param>
    /// <returns>The model builder.</returns>
    public static ModelBuilder LoadConnection<TConnection, TLeft, TRight>(this ModelBuilder builder, IDiscoveryContext? discoveryContext = null)
        where TConnection : class, IModelConnection<TConnection, TLeft, TRight>
        where TLeft : class, IModelConfiguration<TLeft>
        where TRight : class, IModelConfiguration<TRight>
    {
        TConnection.Connect(builder.Entity<TLeft>(), builder.Entity<TRight>());
        discoveryContext?.EntityBuilderCache.Add(typeof(TConnection), builder.Entity<TConnection>());
        return builder;
    }

    /// <summary>
    /// Loads and configures all models that implement <see cref="IReflectiveModelConfiguration{T}"/>.
    /// </summary>
    /// <param name="builder">The model builder.</param>
    /// <param name="namingPolicy">The <see cref="INamingPolicy"/> to be used to determine what action should be taken when no explicit column name is provided.</param>
    /// <param name="mappingPolicy">The <see cref="IMappingPolicy"/> to be used to determine what action should be taken when a property is neither ignored nor explicitly mapped, i.e., how to handle convention-based mapping scenarios.</param>
    /// <returns>The model builder.</returns>
    /// <remarks>
    /// <para>
    /// This method uses reflection to find all types that implement <see cref="IReflectiveModelConfiguration{T}"/> and then loads and configures them.
    /// Models implementing <see cref="IReflectiveModelConfiguration{T}"/> should not be loaded explicitly using <see cref="LoadModel{TModel}(ModelBuilder, IDiscoveryContext)"/>.
    /// </para>
    /// </remarks>
    public static ModelBuilder LoadReflectiveModels(this ModelBuilder builder, INamingPolicy? namingPolicy = null, IMappingPolicy? mappingPolicy = null) =>
        LoadReflectiveModelsInternal(builder, namingPolicy, mappingPolicy, null);

    /// <summary>
    /// Loads and configures all models that implement <see cref="IReflectiveModelConfiguration{T}"/> and are annotated with the specified <typeparamref name="TDatabaseEngineModelAttribute"/>.
    /// </summary>
    /// <typeparam name="TDatabaseEngineModelAttribute">The type data base engine to load the models for.</typeparam>
    /// <param name="builder">The model builder.</param>
    /// <param name="namingPolicy">The <see cref="INamingPolicy"/> to be used to determine what action should be taken when no explicit column name is provided.</param>
    /// <param name="mappingPolicy">The <see cref="IMappingPolicy"/> to be used to determine what action should be taken when a property is neither ignored nor explicitly mapped, i.e., how to handle convention-based mapping scenarios.</param>
    /// <returns>The model builder.</returns>
    /// <remarks>
    /// <para>
    /// This method uses reflection to find all types that implement <see cref="IReflectiveModelConfiguration{T}"/> and then loads and configures them.
    /// Models implementing <see cref="IReflectiveModelConfiguration{T}"/> should not be loaded explicitly using <see cref="LoadModel{TModel}(ModelBuilder, IDiscoveryContext)"/>.
    /// </para>
    /// </remarks>
    public static ModelBuilder LoadReflectiveModels<TDatabaseEngineModelAttribute>(this ModelBuilder builder, INamingPolicy? namingPolicy = null, IMappingPolicy? mappingPolicy = null)
        where TDatabaseEngineModelAttribute : DatabaseEngineModelAttribute, new() =>
        LoadReflectiveModelsInternal(builder, namingPolicy, mappingPolicy, typeof(TDatabaseEngineModelAttribute));

    private static ModelBuilder LoadReflectiveModelsInternal(this ModelBuilder builder, INamingPolicy? namingPolicy, IMappingPolicy? mappingPolicy, Type? dbEngineModelAttributeType)
    {
        _ = builder ?? throw new ArgumentNullException(nameof(builder));
        IDiscoveryContext discoveryContext = new EntityDiscoveryContext(namingPolicy, mappingPolicy);
        ReflectiveModelLoader.LoadAll(builder, discoveryContext, dbEngineModelAttributeType);
        ReflectiveConnectionLoader.LoadAll(builder, discoveryContext, dbEngineModelAttributeType);
        discoveryContext.AuditPolicies();
        return builder;
    }
}