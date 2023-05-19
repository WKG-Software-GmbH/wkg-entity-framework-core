using Wkg.EntityFrameworkCore.ProcedureMapping.Builder;
using Wkg.EntityFrameworkCore.ProcedureMapping.Compiler.Output;

namespace Wkg.EntityFrameworkCore.ProcedureMapping.Compiler;

/// <summary>
/// Represents a compiler capable of translating a procedure builder into an object that can manages the runtime execution of the procedure.
/// </summary>
/// <typeparam name="TCompiledParameter">The concrete type of the compiled parameters of the procedure.</typeparam>
public interface IProcedureCompiler<in TCompiledParameter> where TCompiledParameter : struct, ICompiledParameter
{
    /// <summary>
    /// Compiles the procedure builder into an object that can manages the runtime execution of the procedure.
    /// </summary>
    ICompiledProcedure Compile(TCompiledParameter[] compiledParameters, CompiledResult? compiledResult);
}

/// <summary>
/// Represents the base class for all compilers capable of translating a procedure builder into an object that can manages the runtime execution of the procedure.
/// </summary>
/// <typeparam name="TBuilder"></typeparam>
public abstract class ProcedureCompiler<TBuilder> where TBuilder : IProcedureBuilder
{
    /// <summary>
    /// The procedure builder.
    /// </summary>
    protected TBuilder Builder { get; }

    /// <summary>
    /// The CLR type of the command object representing the interface of this procedure with client code.
    /// </summary>
    protected Type ProcedureType { get; }

    /// <summary>
    /// Creates a new <see cref="ProcedureCompiler{TBuilder}"/> instance.
    /// </summary>
    /// <param name="builder">The procedure builder.</param>
    /// <param name="procedureType">The CLR type of the command object representing the interface of this procedure with client code.</param>
    protected ProcedureCompiler(TBuilder builder, Type procedureType)
    {
        Builder = builder;
        ProcedureType = procedureType;
    }
}