using Microsoft.EntityFrameworkCore;
using System.Reflection;
using System.Runtime.CompilerServices;
using Wkg.EntityFrameworkCore.Configuration.Discovery;
using Wkg.EntityFrameworkCore.Configuration.Reflection.Compat;
using Wkg.EntityFrameworkCore.Configuration.Reflection.Discovery;

namespace Wkg.EntityFrameworkCore.Configuration.Reflection;

/// <summary>
/// Base class for reflective loaders of any kind.
/// </summary>
public abstract class ReflectiveLoaderBase
{
    /// <summary>
    /// Asserts that the loader is only called once.
    /// </summary>
    /// <param name="builder">The model builder.</param>
    /// <param name="sentinel">The sentinel object.</param>
    [MethodImpl(MethodImplOptions.Synchronized)]
    [Obsolete($"This method is obsolete and will be removed in a future version. Use an appropriate implementation of {nameof(IReflectiveDiscoveryContext)} instead.")]
    protected static void AssertLoadOnce(ModelBuilder? builder, ref object? sentinel)
    {
        ArgumentNullException.ThrowIfNull(builder, nameof(builder));
        _ = sentinel ?? throw new InvalidOperationException("ORM model has already been loaded.");
        sentinel = null;
    }

    /// <summary>
    /// Gets all assemblies with an entry point.
    /// </summary>
    /// <returns>The assemblies with an entry point.</returns>
    protected static IEnumerable<Assembly> TargetAssembliesOrWithEntryPoint(Assembly[]? targetAssemblies) => targetAssemblies
        // if no target assemblies are specified, load models from assemblies with an entry point (i.e. not libraries)
        ?? AppDomain.CurrentDomain.GetAssemblies().Where(asm => asm.EntryPoint is not null);

    /// <summary>
    /// Loads all procedures that implement the specified reflective interface.
    /// </summary>
    /// <param name="storedProcedureInterface">The non-generic base interface representing the stored procedure.</param>
    /// <param name="storedProcedure">The generic type of the generic base class representing the stored procedure. This class must implement <paramref name="storedProcedureInterface"/> and specifies one type parameter: the context type which must be a <see langword="class"/> representing the parameter object of the procedure.The type parameters may not be specified when using <see langword="typeof()"/> to call this method.</param>
    /// <param name="reflectiveInterface">The generic interface representing the reflective configuration of the procedure. This interface must be implemented by the procedure type and specifies two type parameters: the procedure type and the context type where the procedure type inherits from <paramref name="storedProcedure"/> and the context type is a <see langword="class"/> representing the parameter object of the procedure. Neither of these type parameters may be specified when using <see langword="typeof()"/> to call this method.</param>
    /// <param name="modelBuilderExtensionsType">The type of the class containing the extension method with the name <paramref name="loadProcedureMethodName"/>.</param>
    /// <param name="loadProcedureMethodName">The name of the extension method to call to load and compile the procedure.</param>
    /// <param name="builder">The <see cref="ModelBuilder"/> instance to use.</param>
    /// <param name="targetAssemblies">The assemblies in which the procedures are defined. If <see langword="null"/>, all assemblies with an entry point will be used.</param>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="storedProcedureInterface"/>, <paramref name="storedProcedure"/>, <paramref name="reflectiveInterface"/>, <paramref name="modelBuilderExtensionsType"/>, or <paramref name="builder"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentException">Thrown if <paramref name="loadProcedureMethodName"/> is <see langword="null"/> or empty.</exception>
    /// <exception cref="InvalidOperationException">Thrown if the procedure type does not implement the reflective interface or if the procedure type does not inherit from the generic base class.</exception>
    [Obsolete($"This method is obsolete and will be removed in a future version. Use an appropriate implementation of {nameof(IReflectiveProcedureDiscoveryContext)} instead.")]
    protected static void LoadAllProceduresInternal(Type storedProcedureInterface, Type storedProcedure, Type reflectiveInterface, Type modelBuilderExtensionsType, string loadProcedureMethodName, ModelBuilder builder, Assembly[]? targetAssemblies)
    {
        IReflectiveProcedureDiscoveryContext discoveryContext = new LegacyProcedureDiscoveryContext();
        IReflectiveProcedureLoader legacyLoader = new LegacyProcedureLoader(storedProcedureInterface, storedProcedure, reflectiveInterface, modelBuilderExtensionsType, loadProcedureMethodName);
        discoveryContext.AddLoader(legacyLoader);
        discoveryContext.Discover(builder, new DiscoveryOptions(targetAssemblies ?? [], []));
    }
}