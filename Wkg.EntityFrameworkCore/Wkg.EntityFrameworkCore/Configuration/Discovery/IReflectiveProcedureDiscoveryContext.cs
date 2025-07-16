using Wkg.EntityFrameworkCore.Configuration.Reflection;

namespace Wkg.EntityFrameworkCore.Configuration.Discovery;

/// <summary>
/// A context for reflective procedure discovery.
/// </summary>
/// <remarks>
/// WARNING: This interface is intended for internal use only and as such is not subject to the same compatibility standards as the public API surface.
/// </remarks>
public interface IReflectiveProcedureDiscoveryContext : IProcedureDiscoveryContext, IReflectiveDiscoveryContext
{
    /// <summary>
    /// Adds the specified <see cref="IReflectiveProcedureLoader"/> to this discovery context.
    /// </summary>
    /// <param name="loader">The loader to add.</param>
    /// <remarks>
    /// WARNING: This method is intended for internal use only and as such is not subject to the same compatibility standards as the public API surface.
    /// </remarks>
    void AddLoader(IReflectiveProcedureLoader loader);
}
