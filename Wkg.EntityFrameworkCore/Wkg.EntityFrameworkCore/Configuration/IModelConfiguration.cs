using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Wkg.EntityFrameworkCore.Configuration;

/// <summary>
/// Represents a model configuration.
/// </summary>
/// <typeparam name="T">The type of the model.</typeparam>
/// <remarks>
/// <para>
/// This interface must be implemented by the entity to be configured.
/// </para>
/// </remarks>
public interface IModelConfiguration<T> where T : class, IModelConfiguration<T>
{
    /// <summary>
    /// Configures the model.
    /// </summary>
    /// <param name="self">The model builder.</param>
    static abstract void Configure(EntityTypeBuilder<T> self);
}