using Wkg.EntityFrameworkCore.Extensions;
using Microsoft.EntityFrameworkCore;
using Wkg.EntityFrameworkCore.Configuration.Reflection.Policies.NamingPolicies;
using Wkg.EntityFrameworkCore.Configuration.Reflection.Policies.MappingPolicies;

namespace Wkg.EntityFrameworkCore.Configuration;

/// <summary>
/// Represents a model configuration that will be reflectively configured by the <see cref="ModelBuilderExtensions.LoadReflectiveModels(ModelBuilder, INamingPolicy, IMappingPolicy)"/> method.
/// </summary>
/// <typeparam name="T">The type of the model.</typeparam>
/// <remarks>
/// <para>
/// This interface must be implemented by the entity to be configured.
/// </para>
/// </remarks>
public interface IReflectiveModelConfiguration<T> : IModelConfiguration<T> where T : class, IReflectiveModelConfiguration<T> { }