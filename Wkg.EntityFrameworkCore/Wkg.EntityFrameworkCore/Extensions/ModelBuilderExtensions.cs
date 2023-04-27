using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Wkg.EntityFrameworkCore.Configuration;
using Wkg.EntityFrameworkCore.Configuration.Reflection;
using Wkg.EntityFrameworkCore.Configuration.Reflection.Attributes;
using Wkg.EntityFrameworkCore.Configuration.Reflection.Policies;
using Wkg.EntityFrameworkCore.Configuration.Reflection.Policies.MappingPolicies;
using Wkg.EntityFrameworkCore.Configuration.Reflection.Policies.NamingPolicies;

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
    /// <param name="namingPolicy">The policy to enforce specifying how parameter names are mapped to column names.</param>
    /// <param name="propertyMappingPolicy">The policy to enforce specifying how to handle properties that are not explicitly mapped.</param>
    /// <returns>The model builder.</returns>
    /// <remarks>
    /// <para>
    /// This method uses reflection to find all types that implement <see cref="IReflectiveModelConfiguration{T}"/> and then loads and configures them.
    /// Models implementing <see cref="IReflectiveModelConfiguration{T}"/> should not be loaded using <see cref="LoadModel{TModel}(ModelBuilder)"/>.
    /// </para>
    /// </remarks>
    public static ModelBuilder LoadReflectiveModels(this ModelBuilder builder, INamingPolicy? namingPolicy = null, IMappingPolicy? propertyMappingPolicy = null)
    {
        namingPolicy ??= EntityNamingPolicy.PreferExplicit;
        propertyMappingPolicy ??= PropertyMappingPolicy.IgnoreImplicit;
        _ = builder ?? throw new ArgumentNullException(nameof(builder));
        IReadOnlyDictionary<Type, EntityTypeBuilder> builderCache = ReflectiveModelLoader.LoadAll(builder, null, namingPolicy, propertyMappingPolicy);
        ReflectiveConnectionLoader.LoadAll(builder, builderCache, null);
        return builder;
    }

    /// <summary>
    /// Loads and configures all models that implement <see cref="IReflectiveModelConfiguration{T}"/> and are annotated with the specified <typeparamref name="TDatabaseEngineModelAttribute"/>.
    /// </summary>
    /// <typeparam name="TDatabaseEngineModelAttribute">The type data base engine to load the models for.</typeparam>
    /// <param name="builder">The model builder.</param>
    /// <param name="namingPolicy">The policy to enforce specifying how parameter names are mapped to column names.</param>
    /// <param name="propertyMappingPolicy">The policy to enforce specifying how to handle properties that are not explicitly mapped.</param>
    /// <returns>The model builder.</returns>
    /// <remarks>
    /// <para>
    /// This method uses reflection to find all types that implement <see cref="IReflectiveModelConfiguration{T}"/> and then loads and configures them.
    /// Models implementing <see cref="IReflectiveModelConfiguration{T}"/> should not be loaded using <see cref="LoadModel{TModel}(ModelBuilder)"/>.
    /// </para>
    /// </remarks>
    public static ModelBuilder LoadReflectiveModels<TDatabaseEngineModelAttribute>(this ModelBuilder builder, INamingPolicy? namingPolicy = null, IMappingPolicy? propertyMappingPolicy = null)
        where TDatabaseEngineModelAttribute : DatabaseEngineModelAttribute, new()
    {
        namingPolicy ??= EntityNamingPolicy.PreferExplicit;
        propertyMappingPolicy ??= PropertyMappingPolicy.IgnoreImplicit;
        _ = builder ?? throw new ArgumentNullException(nameof(builder));
        IReadOnlyDictionary<Type, EntityTypeBuilder> builderCache = ReflectiveModelLoader.LoadAll(builder, typeof(TDatabaseEngineModelAttribute), namingPolicy, propertyMappingPolicy);
        ReflectiveConnectionLoader.LoadAll(builder, builderCache, typeof(TDatabaseEngineModelAttribute));
        return builder;
    }
}