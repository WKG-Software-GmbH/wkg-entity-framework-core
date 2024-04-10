using System.Reflection;

namespace Wkg.EntityFrameworkCore.Configuration.Reflection.Discovery;

/// <summary>
/// Configures global entity discovery options.
/// </summary>
/// <param name="TargetAssemblies">The assemblies to be included in entity discovery.</param>
/// <param name="TargetDatabaseEngineAttributes">The database engine attributes to be included in entity discovery, or <see cref="Array.Empty{T}"/> to load all database engines.</param>
public record DiscoveryOptions(Assembly[] TargetAssemblies, Type[] TargetDatabaseEngineAttributes);
