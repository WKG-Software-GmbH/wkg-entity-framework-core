using Microsoft.EntityFrameworkCore.Metadata;

namespace Wkg.EntityFrameworkCore.Configuration.Policies.ColumnNamingPolicies;

/// <summary>
/// A policy that can be applied to an <see cref="IMutableEntityType" /> to enforce a specific naming guideline. 
/// An <see cref="IColumnNamingPolicy"/> determines what action should be taken when no explicit column or table name is provided.
/// </summary>
public interface IColumnNamingPolicy : IEntityPolicy
{
}