namespace Wkg.EntityFrameworkCore.ProcedureMapping.Compiler.Output;

/// <summary>
/// Represents a property get accessor, i.e. a delegate that can be used to read the value of a property from the provided I/O <paramref name="container"/> object.
/// </summary>
/// <param name="container">The I/O container object owning the property to be read.</param>
/// <returns>The value of the property.</returns>
public delegate object PropertyGetter(object container);

/// <summary>
/// Represents a property set accessor, i.e. a delegate that can be used to write the value of a property of the provided I/O <paramref name="container"/> object.
/// </summary>
/// <param name="container">The I/O container object owning the property to be updated.</param>
/// <param name="newValue">The new value of the property.</param>
/// <remarks>
/// The type of <paramref name="newValue"/> is <see cref="object"/> because all bindings are IL-emitted at runtime.
/// </remarks>
public delegate void PropertySetter(object container, object? newValue);

/// <summary>
/// Sets a property on the I/O <paramref name="container"/> object to the unboxed/correctly typed value provided in <paramref name="newValue"/>.
/// </summary>
/// <remarks>
/// No type checks are performed.
/// YOU have to ensure that the type of <paramref name="newValue"/> is correct.
/// </remarks>
/// <typeparam name="T">The type of the value to be set.</typeparam>
/// <param name="container">The I/O container object owning the property to be updated.</param>
/// <param name="newValue">The new value of the property.</param>
public delegate void PropertySetter<T>(object container, T newValue);