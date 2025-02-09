using Microsoft.EntityFrameworkCore;
using Wkg.EntityFrameworkCore.Configuration.Reflection.Discovery;

namespace Wkg.EntityFrameworkCore.Configuration.Discovery;

/// <summary>
/// A context used for reflective ORM discovery.
/// </summary>
/// <remarks>
/// WARNING: This interface is intended for internal use only and as such is not subject to the same compatibility standards as the public API surface.
/// </remarks>
public interface IReflectiveDiscoveryContext
{
    /// <summary>
    /// Reflectively discovers ORM elements and adds them to the specified <paramref name="builder"/>.
    /// </summary>
    /// <remarks>
    /// WARNING: This interface is intended for internal use only and as such is not subject to the same compatibility standards as the public API surface.
    /// </remarks>
    /// <param name="builder">The <see cref="ModelBuilder"/> to add the discovered ORM elements to.</param>
    /// <param name="options">The options to use for discovery.</param>
    void Discover(ModelBuilder builder, DiscoveryOptions options);
}