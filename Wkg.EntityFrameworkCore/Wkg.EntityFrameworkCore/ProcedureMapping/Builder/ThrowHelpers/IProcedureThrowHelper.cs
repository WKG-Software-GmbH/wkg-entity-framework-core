using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;

namespace Wkg.EntityFrameworkCore.ProcedureMapping.Builder.ThrowHelpers;

public interface IProcedureThrowHelper : IThrowHelper
{
    IThrowHelper ForParameter(PropertyInfo property);

    IResultThrowHelper ForResult<TResult>();
}
