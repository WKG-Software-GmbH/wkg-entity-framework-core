namespace Wkg.EntityFrameworkCore.Configuration.Reflection.Compat;

[Obsolete($"This class is only used for compatibility with legacy code and will be removed in a future version. Use an appropriate implementation of {nameof(ReflectiveProcedureLoader)} instead.")]
internal class LegacyProcedureLoader(Type storedProcedureInterface, Type storedProcedure, Type reflectiveInterface, Type modelBuilderExtensionsType, string loadProcedureMethodName) 
    : ReflectiveProcedureLoader
{
    protected override StoredPrecedureLoaderOptions Options { get; } = new
    (
        storedProcedureInterface,
        storedProcedure,
        reflectiveInterface,
        modelBuilderExtensionsType,
        loadProcedureMethodName
    );
}
