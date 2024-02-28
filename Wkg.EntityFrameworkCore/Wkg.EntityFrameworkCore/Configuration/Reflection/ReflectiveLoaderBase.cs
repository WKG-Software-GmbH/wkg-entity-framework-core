using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.CompilerServices;
using Wkg.Common.Extensions;
using Wkg.Extensions.Reflection;
using Wkg.Logging;

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
    protected static void AssertLoadOnce(ModelBuilder? builder, ref object? sentinel)
    {
        _ = builder ?? throw new ArgumentNullException(nameof(builder));
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
    protected static void LoadAllProceduresInternal(Type storedProcedureInterface, Type storedProcedure, Type reflectiveInterface, Type modelBuilderExtensionsType, string loadProcedureMethodName, ModelBuilder builder, Assembly[]? targetAssemblies)
    {
        Log.WriteInfo($"Loading all procedures implementing {storedProcedureInterface.Name}.");
        ReflectiveProcedure[] entities = TargetAssembliesOrWithEntryPoint(targetAssemblies)
            // get all types in these assemblies
            .SelectMany(asm => asm.GetExportedTypes()
                .Where(type =>
                    // only keep classes
                    type.IsClass
                    // first quick filter by interface
                    && type.ImplementsInterface(storedProcedureInterface)))
            // just to be sure ...
            .Distinct()
            .Select(type => new ReflectiveProcedure
            (
                ProcedureType: type,
                // get the I/O Container type argument of StoredProcedure<TIOContainer> if the type is derived from StoredProcedure<TIOContainer>, otherwise get null
                ContextType: type.GetGenericBaseClassTypeArgument(storedProcedure)
            ))
            .Where(procedure =>
                // procedure must not be abstract
                !procedure.ProcedureType.IsAbstract
                // verify that the context type is a non-null reference type
                && (procedure.ContextType?.IsClass ?? false)
                // check if the type implements IReflectiveProcedureConfiguration<TProcedure, TIOContainer> where TProcedure is StoredProcedure<T>
                // and TIOContainer is T.
                // We need to check this because the type may be derived from StoredProcedure<T> but it might not
                // implement the reflective interface.
                && procedure.ProcedureType.ImplementsDirectGenericInterfaceWithTypeParameters(reflectiveInterface, procedure.ProcedureType, procedure.ContextType))
            .ToArray();

        Log.WriteInfo($"Discovered {entities.Length} stored procedures.");

        // re-use parameter array for all procedures
        object?[] parameters = new object[1];
        foreach (ReflectiveProcedure entity in entities)
        {
            Log.WriteDiagnostic($"Loading: {entity.ProcedureType.Name}.");
            // use the load procedure extension method to configure and compile the procedure
            MethodInfo? loadProcedure = modelBuilderExtensionsType.GetMethod
            (
                loadProcedureMethodName,                                                // method name
                2,                                                                      // number of generic type arguments
                BindingFlags.Static | BindingFlags.Public | BindingFlags.DeclaredOnly,  // binding flags
                null,                                                                   // binder (null = default)
                [typeof(ModelBuilder)],                                                 // parameter types
                null                                                                    // modifiers (null = default)
            );
            // make the method generic
            MethodInfo genericLoadProcedure = loadProcedure!
                .MakeGenericMethod(entity.ProcedureType, entity.ContextType!);
            parameters[0] = builder;
            // invoke the method
            genericLoadProcedure.Invoke(null, parameters);
            Log.WriteDiagnostic($"Loaded: {entity.ProcedureType.Name}.");
        }
        Log.WriteInfo($"Loaded {entities.Length} stored procedures.");
    }
}