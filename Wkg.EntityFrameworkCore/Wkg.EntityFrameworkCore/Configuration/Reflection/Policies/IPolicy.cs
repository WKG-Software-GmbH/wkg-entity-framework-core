using Microsoft.EntityFrameworkCore.Metadata;

namespace Wkg.EntityFrameworkCore.Configuration.Reflection.Policies;

public interface IPolicy
{
    internal void Audit(IMutableEntityType entityType);
}