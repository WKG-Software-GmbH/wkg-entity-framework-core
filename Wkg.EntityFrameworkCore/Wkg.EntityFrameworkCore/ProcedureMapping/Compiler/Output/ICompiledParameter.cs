using System.Data;
using System.Data.Common;
using Wkg.EntityFrameworkCore.ProcedureMapping.Compiler;

namespace Wkg.EntityFrameworkCore.ProcedureMapping.Compiler.Output;

public interface ICompiledParameter
{
    public string Name { get; }

    public ParameterDirection Direction { get; }

    public PropertyGetter Getter { get; }

    public PropertySetter? Setter { get; }

    public void Load(ref DbParameter? parameter, object context);

    public void Store(ref DbParameter param, object context);

    public bool IsOutput { get; }
}