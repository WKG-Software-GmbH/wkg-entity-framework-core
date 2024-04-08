using Microsoft.EntityFrameworkCore.Metadata;

namespace Wkg.EntityFrameworkCore.Configuration.Policies;

/// <summary>
/// A policy that can be applied to an <see cref="IMutableEntityType" /> to enforce a specific guideline.
/// </summary>
public interface IEntityPolicy
{
    /// <summary>
    /// Audits the specified <see cref="IMutableEntityType"/> for compliance with the policy and takes corresponding actions if necessary.
    /// </summary>
    /// <param name="entityType">The <see cref="IMutableEntityType"/> to audit.</param>
    void Audit(IMutableEntityType entityType);
}