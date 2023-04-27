using System.Reflection;

namespace Wkg.EntityFrameworkCore.ProcedureMapping.Builder.ThrowHelpers;

public interface IResultThrowHelper : IThrowHelper
{
    IThrowHelper ForColumn(PropertyInfo property);
}
