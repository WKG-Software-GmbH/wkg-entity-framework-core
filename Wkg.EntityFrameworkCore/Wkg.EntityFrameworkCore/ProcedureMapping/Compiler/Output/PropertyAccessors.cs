namespace Wkg.EntityFrameworkCore.ProcedureMapping.Compiler.Output;

public delegate object PropertyGetter(object context);

public delegate void PropertySetter(object context, object? newValue);

/// <summary>
/// Sets a property on the <paramref name="context"/> object to the unboxed/correctly typed value provided in <paramref name="newValue"/>. No type checks are performed. YOU have to ensure that the type of <paramref name="newValue"/> is correct.
/// </summary>
/// <typeparam name="T"></typeparam>
/// <param name="context"></param>
/// <param name="newValue"></param>
public delegate void PropertySetter<T>(object context, T newValue);