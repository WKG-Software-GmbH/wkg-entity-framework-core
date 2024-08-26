using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Wkg.EntityFrameworkCore.Configuration;
using Wkg.EntityFrameworkCore.Configuration.Reflection;
using Wkg.EntityFrameworkCore.Configuration.Reflection.Discovery;
using Wkg.EntityFrameworkCore.Configuration.Discovery;
using Wkg.EntityFrameworkCore.Configuration.Policies;
using Wkg.EntityFrameworkCore.Configuration.Policies.Defaults.PropertyMappingPolicies;
using Wkg.EntityFrameworkCore.Configuration.Policies.Defaults.EntityNamingPolicies;

namespace Wkg.EntityFrameworkCore.Extensions;

/// <summary>
/// Holds extension methods for <see cref="ModelBuilder"/>.
/// </summary>
public static class ModelBuilderExtensions
{
    /// <summary>
    /// Initializes a new <see cref="IDiscoveryContext"/> using the specified <paramref name="policies"/>.
    /// </summary>
    /// <param name="_">The model builder.</param>
    /// <param name="policies">The policies to be enforced on the discovered entities.</param>
    /// <returns>The <see cref="IDiscoveryContext"/>.</returns>
    public static IDiscoveryContext CreateDiscoveryContext(this ModelBuilder _, IEntityPolicy[] policies) => 
        new EntityDiscoveryContext(policies);

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
    /// <param name="configureOptions">The options to configure the discovery process.</param>
    /// <returns>The model builder.</returns>
    /// <remarks>
    /// <para>
    /// This method uses reflection to find all types that implement <see cref="IReflectiveModelConfiguration{T}"/> and then loads and configures them.
    /// Models implementing <see cref="IReflectiveModelConfiguration{T}"/> should not be loaded explicitly using <see cref="LoadModel{TModel}(ModelBuilder, IDiscoveryContext)"/>.
    /// </para>
    /// </remarks>
    public static ModelBuilder LoadReflectiveModels(this ModelBuilder builder, Action<IModelOptionsBuilder>? configureOptions) =>
        LoadReflectiveModelsInternal(builder, configureOptions);

    private static ModelBuilder LoadReflectiveModelsInternal(this ModelBuilder builder, Action<IModelOptionsBuilder>? configureOptions)
    {
        ArgumentNullException.ThrowIfNull(builder, nameof(builder));

        ModelOptionsBuilder modelOptions = new();
        configureOptions?.Invoke(modelOptions);
        modelOptions.AddDefaults();
        IEntityPolicy[] policies = modelOptions.PolicyOptionsBuilder.Build();
        DiscoveryOptions discoveryOptions = modelOptions.DiscoveryOptionsBuilder.Build();

        EntityDiscoveryContext discoveryContext = new(policies);
        ReflectiveModelLoader.LoadAll(builder, discoveryContext, discoveryOptions);
        ReflectiveConnectionLoader.LoadAll(builder, discoveryContext, discoveryOptions);
        discoveryContext.AuditPolicies();
        return builder;
    }

    private static void AddDefaults(this ModelOptionsBuilder modelOptions)
    {
        EntityNaming.AddDefaults(modelOptions.PolicyOptionsBuilder);
        PropertyMapping.AddDefaults(modelOptions.PolicyOptionsBuilder);
    }
}