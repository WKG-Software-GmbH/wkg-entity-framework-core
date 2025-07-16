using Microsoft.EntityFrameworkCore;
using Wkg.EntityFrameworkCore.Configuration.Discovery;
using Wkg.EntityFrameworkCore.Configuration.Reflection.Discovery;

namespace Wkg.EntityFrameworkCore.Configuration.Reflection;

/// <summary>
/// Represents a loader that dynamically discovers and registers stored procedures with the ORM model.
/// </summary>
/// <remarks>
/// WARNING: This interface is intended for internal use only and as such is not subject to the same compatibility standards as the public API surface.
/// </remarks>
public interface IReflectiveProcedureLoader
{
    internal void LoadProcedures(ModelBuilder builder, IProcedureDiscoveryContext discoveryContext, DiscoveryOptions options);
}