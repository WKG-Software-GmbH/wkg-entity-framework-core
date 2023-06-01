using System.Data;
using System.Data.Common;

namespace Wkg.EntityFrameworkCore.ProcedureMapping.Compiler.Output;

/// <summary>
/// Represents a compiled parameter of a procedure.
/// </summary>
public interface ICompiledParameter
{
    /// <summary>
    /// The database name of the parameter.
    /// </summary>
    public string Name { get; }

    /// <summary>
    /// The direction of the parameter.
    /// </summary>
    public ParameterDirection Direction { get; }

    /// <summary>
    /// The generated get accessor used to read the parameter value from the I/O container before procedure execution.
    /// </summary>
    public PropertyGetter Getter { get; }

    /// <summary>
    /// The generated set accessor used to write the parameter value back into the I/O container after procedure execution.
    /// </summary>
    public PropertySetter? Setter { get; }

    /// <summary>
    /// Loads the value of the parameter from the I/O <paramref name="container"/> object into the ADO.NET <see cref="DbParameter"/> <paramref name="parameter"/> using the generated <see cref="Getter"/>.
    /// </summary>
    /// <param name="parameter">The ADO.NET <see cref="DbParameter"/> to be loaded.</param>
    /// <param name="container">The I/O container object holding the value.</param>
    public void Load(ref DbParameter? parameter, object container);

    /// <summary>
    /// Stores the value of the ADO.NET <see cref="DbParameter"/> <paramref name="parameter"/> after execution in the I/O <paramref name="container"/> object using the generated <see cref="Setter"/>.
    /// </summary>
    /// <param name="parameter">The ADO.NET <see cref="DbParameter"/> containing the value to be stored.</param>
    /// <param name="container">The I/O container object used to return the value back to the caller.</param>
    public void Store(ref DbParameter parameter, object container);

    /// <summary>
    /// Indicates whether the value of this parameter needs to be updated after procedure execution.
    /// </summary>
    public bool IsOutput { get; }
}