using Microsoft.EntityFrameworkCore;
using Wkg.EntityFrameworkCore.Configuration.Discovery;
using Wkg.EntityFrameworkCore.Configuration.Reflection.Discovery;

namespace Wkg.EntityFrameworkCore.Configuration.Reflection;

internal interface IReflectiveEntityLoader
{
    void LoadEntities(ModelBuilder builder, IEntityDiscoveryContext discoveryContext, DiscoveryOptions options);
}