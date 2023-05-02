using Microsoft.EntityFrameworkCore.Metadata;

namespace Wkg.EntityFrameworkCore.Configuration.Reflection.Policies;

/// <summary>
/// A policy that can be applied to an <see cref="IMutableEntityType" /> to enforce a specific guideline.
/// </summary>
public interface IPolicy
{
    internal void Audit(IMutableEntityType entityType);
}