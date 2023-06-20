using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Wkg.EntityFrameworkCore.Configuration;
using Wkg.EntityFrameworkCore.Configuration.Reflection;
using Wkg.EntityFrameworkCore.Configuration.Reflection.Attributes;
using Wkg.EntityFrameworkCore.Configuration.Reflection.Policies;
using Wkg.EntityFrameworkCore.Configuration.Reflection.Policies.MappingPolicies;
using Wkg.EntityFrameworkCore.Configuration.Reflection.Policies.NamingPolicies;
using Wkg.Logging;

namespace Wkg.EntityFrameworkCore.Extensions;

/// <summary>
/// Holds extension methods for <see cref="ModelBuilder"/>.
/// </summary>
public static class ModelBuilderExtensions
{
    /// <summary>
    /// Loads and configures the specified <typeparamref name="TModel"/>.
    /// </summary>
    /// <typeparam name="TModel">The type of the model.</typeparam>
    /// <param name="builder">The model builder.</param>
    /// <returns>The model builder.</returns>
    public static ModelBuilder LoadModel<TModel>(this ModelBuilder builder)
        where TModel : class, IModelConfiguration<TModel>
    {
        TModel.Configure(builder.Entity<TModel>());
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
    /// Models implementing <see cref="IReflectiveModelConfiguration{T}"/> should not be loaded explicitly using <see cref="LoadModel{TModel}(ModelBuilder)"/>.
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
    /// Models implementing <see cref="IReflectiveModelConfiguration{T}"/> should not be loaded explicitly using <see cref="LoadModel{TModel}(ModelBuilder)"/>.
    /// </para>
    /// </remarks>
    public static ModelBuilder LoadReflectiveModels<TDatabaseEngineModelAttribute>(this ModelBuilder builder, INamingPolicy? namingPolicy = null, IMappingPolicy? mappingPolicy = null)
        where TDatabaseEngineModelAttribute : DatabaseEngineModelAttribute, new() =>
        LoadReflectiveModelsInternal(builder, namingPolicy, mappingPolicy, typeof(TDatabaseEngineModelAttribute));

    private static ModelBuilder LoadReflectiveModelsInternal(this ModelBuilder builder, INamingPolicy? namingPolicy, IMappingPolicy? mappingPolicy, Type? dbEngineModelAttributeType)
    {
        namingPolicy ??= EntityNamingPolicy.PreferExplicit;
        mappingPolicy ??= PropertyMappingPolicy.IgnoreImplicit;
        _ = builder ?? throw new ArgumentNullException(nameof(builder));
        IReadOnlyDictionary<Type, EntityTypeBuilder> builderCache = ReflectiveModelLoader.LoadAll(builder, dbEngineModelAttributeType);
        ReflectiveConnectionLoader.LoadAll(builder, builderCache, dbEngineModelAttributeType);

        // now that all models are configured, we can audit compliance with the specified policies
        Log.WriteInfo($"Auditing {builderCache.Count} models for compliance with the specified policies.");
        foreach (EntityTypeBuilder entityType in builderCache.Values)
        {
            mappingPolicy.Audit(entityType.Metadata);
            namingPolicy.Audit(entityType.Metadata);
        }
        Log.WriteInfo($"Audit completed. {builderCache.Count} models loaded and configured.");
        return builder;
    }
}