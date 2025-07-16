using Microsoft.EntityFrameworkCore;
using System.Reflection;
using Wkg.EntityFrameworkCore.Configuration.Discovery;
using Wkg.EntityFrameworkCore.Configuration.Reflection.Discovery;
using Wkg.Logging;
using Wkg.Reflection.Extensions;

namespace Wkg.EntityFrameworkCore.Configuration.Reflection;

/// <summary>
/// Represents the base class for reflective procedure loaders.
/// </summary>
public abstract class ReflectiveProcedureLoader : ReflectiveLoaderBase, IReflectiveProcedureLoader
{
    /// <summary>
    /// The options for stored procedure discovery and loading.
    /// </summary>
    protected abstract StoredPrecedureLoaderOptions Options { get; }

    void IReflectiveProcedureLoader.LoadProcedures(ModelBuilder builder, IProcedureDiscoveryContext discoveryContext, DiscoveryOptions discoveryOptions)
    {
        StoredPrecedureLoaderOptions options = Options;
        Assembly[]? targetAssemblies = null;
        if (discoveryOptions.TargetAssemblies.Length > 0)
        {
            targetAssemblies = discoveryOptions.TargetAssemblies;
        }
        Log.WriteInfo($"{nameof(ReflectiveProcedureLoader)} is initializing.");

        Log.WriteInfo($"Loading all procedures implementing {options.StoredProcedureInterface.Name}.");
        ReflectiveProcedure[] entities = TargetAssembliesOrWithEntryPoint(targetAssemblies)
            // get all types in these assemblies
            .SelectMany(asm => asm.GetTypes()
                .Where(type =>
                    // only keep classes
                    type.IsClass
                    // first quick filter by interface
                    && type.ImplementsInterface(options.StoredProcedureInterface)))
            // just to be sure ...
            .Distinct()
            .Select(type => new ReflectiveProcedure
            (
                ProcedureType: type,
                // get the I/O Container type argument of StoredProcedure<TIOContainer> if the type is derived from StoredProcedure<TIOContainer>, otherwise get null
                ContextType: type.GetGenericBaseClassTypeArgument(options.StoredProcedure)
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
                && procedure.ProcedureType.ImplementsDirectGenericInterfaceWithTypeParameters(options.ReflectiveInterface, procedure.ProcedureType, procedure.ContextType))
            .ToArray();

        Log.WriteInfo($"Discovered {entities.Length} stored procedures.");

        // use the load procedure extension method to configure and compile the procedure
        MethodInfo loadProcedure = options.ModelBuilderExtensionsType.GetMethod
        (
            name: options.LoadProcedureMethodName,
            genericParameterCount: 2,
            bindingAttr: BindingFlags.Static | BindingFlags.Public | BindingFlags.DeclaredOnly,
            binder: null,
            types: [typeof(ModelBuilder)],
            modifiers: null
        ) ?? throw new InvalidOperationException($"No method found that matches the specified signature: {options.ModelBuilderExtensionsType.Name}.{options.LoadProcedureMethodName}<TProcedure, TIOContainer>(ModelBuilder).");
        // re-use parameter array for all procedures
        object?[] parameters = new object[1];
        foreach (ReflectiveProcedure entity in entities)
        {
            Log.WriteDiagnostic($"Loading: {entity.ProcedureType.Name}.");
            // make the method generic
            MethodInfo genericLoadProcedure = loadProcedure
                .MakeGenericMethod(entity.ProcedureType, entity.ContextType!);
            parameters[0] = builder;
            // invoke the method
            genericLoadProcedure.Invoke(null, parameters);
            Log.WriteDiagnostic($"Loaded: {entity.ProcedureType.Name}.");
        }
        Log.WriteInfo($"Loaded {entities.Length} stored procedures.");
    }

    /// <summary>
    /// The internal options for stored procedure discovery and loading.
    /// </summary>
    /// <param name="StoredProcedureInterface">The non-generic base interface representing the stored procedure.</param>
    /// <param name="StoredProcedure">The generic type of the generic base class representing the stored procedure. This class must implement <paramref name="StoredProcedureInterface"/> and specifies one type parameter: the context type which must be a <see langword="class"/> representing the parameter object of the procedure.</param>
    /// <param name="ReflectiveInterface">The generic interface representing the reflective configuration of the procedure. This interface must be implemented by the procedure type and specifies two type parameters: the procedure type and the context type where the procedure type inherits from <paramref name="StoredProcedure"/> and the context type is a <see langword="class"/> representing the parameter object of the procedure.</param>
    /// <param name="ModelBuilderExtensionsType">The type of the class containing the extension method with the name <paramref name="LoadProcedureMethodName"/>.</param>
    /// <param name="LoadProcedureMethodName">The name of the extension method to call to load and compile the procedure.</param>
    internal protected record StoredPrecedureLoaderOptions
    (
        Type StoredProcedureInterface,
        Type StoredProcedure,
        Type ReflectiveInterface,
        Type ModelBuilderExtensionsType,
        string LoadProcedureMethodName
    );
}
