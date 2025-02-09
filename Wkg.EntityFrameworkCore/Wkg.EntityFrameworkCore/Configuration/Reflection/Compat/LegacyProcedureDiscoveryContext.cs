using Microsoft.EntityFrameworkCore;
using Wkg.EntityFrameworkCore.Configuration.Discovery;

namespace Wkg.EntityFrameworkCore.Configuration.Reflection.Compat;

[Obsolete($"This class is only used for compatibility with legacy code and will be removed in a future version. Use an appropriate implementation of {nameof(ProcedureDiscoveryContext)} instead.")]
internal class LegacyProcedureDiscoveryContext : ProcedureDiscoveryContext
{
    protected override List<WeakReference<ModelBuilder>> StaticModelBuilderCache => [];
}
