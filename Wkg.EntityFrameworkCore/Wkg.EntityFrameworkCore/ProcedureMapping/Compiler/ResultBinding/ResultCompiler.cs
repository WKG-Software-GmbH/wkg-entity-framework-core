using System.Data.Common;
using System.Linq.Expressions;
using System.Reflection;
using Wkg.EntityFrameworkCore.ProcedureMapping.Builder.ResultBinding;
using Wkg.EntityFrameworkCore.ProcedureMapping.Compiler.Output;
using Wkg.Reflection;

namespace Wkg.EntityFrameworkCore.ProcedureMapping.Compiler.ResultBinding;

/// <summary>
/// Represents a compiler capable of transforming all pre-compiled result columns and the configured result type into executable IL code, allowing a result entity to be constructed from a <typeparamref name="TDataReader"/> row.
/// </summary>
/// <typeparam name="TDataReader">The type of the <see cref="DbDataReader"/> to read the result from.</typeparam>
public interface IResultCompiler<TDataReader>
    where TDataReader : DbDataReader
{
    /// <summary>
    /// Compiles all bindings into executable IL code.
    /// </summary>
    CompiledResult<TDataReader> Compile(CompiledResultColumn[] compiledResultColumns);
}

/// <summary>
/// Represents a compiler capable of transforming all pre-compiled result columns and the result entity configured by the <typeparamref name="TBuilder"/> into executable IL code, allowing a result entity to be constructed from a <see cref="DbDataReader"/> row.
/// </summary>
/// <typeparam name="TBuilder">The type of the <see cref="IResultBuilder"/> used to configure the result entity.</typeparam>
/// <remarks>
/// Creates a new <see cref="ResultCompiler{TBuilder}"/> instance.
/// </remarks>
/// <param name="builder">The <see cref="IResultBuilder"/> used to configure the result entity.</param>
public abstract class ResultCompiler<TBuilder>(TBuilder builder) where TBuilder : IResultBuilder
{
    /// <summary>
    /// The <see cref="IResultBuilder"/> used to configure the result entity.
    /// </summary>
    protected TBuilder Builder { get; } = builder;

    /// <summary>
    /// Compiles all result column bindings into executable IL code, and calls the matching constructor of the result entity with the compiled result columns as arguments.
    /// </summary>
    /// <typeparam name="TDataReader">The type of the <see cref="DbDataReader"/> to read the result from.</typeparam>
    /// <param name="compiledResultColumns">The pre-compiled result columns.</param>
    /// <returns>A <see cref="CompiledResult{TDataReader}"/> instance that can be used to construct a result entity from a <typeparamref name="TDataReader"/> row.</returns>
    /// <exception cref="InvalidOperationException">Thrown when the result entity has no matching constructor for the mapped columns.</exception>
    /// <exception cref="InvalidOperationException">Thrown when the result entity has a constructor with a different number of parameters than the mapped columns.</exception>
    protected CompiledResultFactory<TDataReader> CompileResultFactoryFor<TDataReader>(CompiledResultColumn[] compiledResultColumns)
        where TDataReader : DbDataReader
    {
        // get all constructors of the result type
        ConstructorInfo[] ctors = Builder.ResultClrType.GetConstructors(
            BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.DeclaredOnly | BindingFlags.Instance);

        // find the one (if any) constructor that matches the mapped columns
        ConstructorInfo? constructor = ctors.SingleOrDefault(ctor =>
            ctor.GetParameters().All(param =>
                compiledResultColumns.Any(clmn =>
                    clmn.PropertyName.Equals(param.Name, StringComparison.InvariantCultureIgnoreCase)
                    && clmn.PropertyClrType.Equals(param.ParameterType))));

        _ = constructor ?? Builder.ThrowHelper.Throw<InvalidOperationException, ConstructorInfo>($"Return type '{Builder.ResultClrType.Name}' has no matching constructor for the mapped columns! Did you miss a mapping?");

        // map the configured columns to the constructor parameters
        ParameterInfo[] parameters = constructor.GetParameters();

        // this should never happen, but just in case
        if (parameters.Length != compiledResultColumns.Length)
        {
            Builder.ThrowHelper.Throw<InvalidOperationException>($"Return type '{Builder.ResultClrType.Name}' has a constructor of Length {parameters.Length}, but {compiledResultColumns.Length} columns were mapped!");
        }

        // create a lambda expression that invokes the constructor with the mapped columns as arguments in the correct order
        ParameterExpression reader = Expression.Parameter(typeof(TDataReader), "reader");
        Expression[] argumentExpressions = new Expression[parameters.Length];

        for (int i = 0; i < parameters.Length; i++)
        {
            for (int j = 0; j < compiledResultColumns.Length; j++)
            {
                // if the parameter name and type match the mapped column, invoke the column factory and use the result as argument
                if (parameters[i].Name!.Equals(compiledResultColumns[j].PropertyName, StringComparison.InvariantCultureIgnoreCase)
                    && parameters[i].ParameterType.Equals(compiledResultColumns[j].PropertyClrType))
                {
                    // this invoke expression will inline the column factory call and use the result as argument
                    argumentExpressions[i] = Expression.Invoke(compiledResultColumns[j].ColumnFactory, reader);
                }
            }
        }

        // invoke the constructor with the mapped columns as arguments
        NewExpression ctorInvocation = Expression.New(constructor, argumentExpressions);

        // we only allow reference types as result types, so we can safely cast the result to object without checks
        MethodInfo unsafeAs = UnsafeReflection.As(typeof(object));
        MethodCallExpression result = Expression.Call(unsafeAs, ctorInvocation);

        // compile the lambda expression to a delegate
        return Expression.Lambda<CompiledResultFactory<TDataReader>>(result, reader).Compile();
    }
}