using Wkg.EntityFrameworkCore.Configuration.Reflection;

namespace Wkg.EntityFrameworkCore.Configuration.Discovery;

internal interface IReflectiveEntityDiscoveryContext : IEntityDiscoveryContext, IReflectiveDiscoveryContext
{
    void AddLoader(IReflectiveEntityLoader loader);
}
