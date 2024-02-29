using System.Reflection;

namespace Wkg.EntityFrameworkCore.Configuration.Reflection.Discovery;

/// <summary>
/// Configures global entity discovery options.
/// </summary>
/// <param name="TargetAssemblies">The assemblies to be included in entity discovery.</param>
public record DiscoveryOptions(Assembly[] TargetAssemblies);
