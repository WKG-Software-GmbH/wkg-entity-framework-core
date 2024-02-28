using System.Reflection;

namespace Wkg.EntityFrameworkCore.Configuration.Reflection.Discovery;

/// <summary>
/// Represents a targeted assembly for entity discovery.
/// </summary>
public interface ITargetAssembly
{
    /// <summary>
    /// Returns the assembly containing the entity types to be enumerated.
    /// </summary>
    internal static abstract Assembly Assembly { get; }
}
