using System.Data.Common;
using System.Linq.Expressions;
using Wkg.EntityFrameworkCore.ProcedureMapping.Builder.ResultBinding;
using Wkg.EntityFrameworkCore.ProcedureMapping.Compiler.Output;

namespace Wkg.EntityFrameworkCore.ProcedureMapping.Compiler.ResultBinding;

/// <summary>
/// Represents a compiler capable of transforming the configured result column binding into an expression tree.
/// </summary>
public interface IResultColumnCompiler
{
    /// <summary>
    /// Precompiles the configured result column bindings into an expression tree to read the column value from the <see cref="DbDataReader"/>, perform null checks and convert the value to the desired type using the configured conversion expressions.
    /// </summary>
    CompiledResultColumn Compile();
}

/// <summary>
/// Represents the base class for all compilers capable of transforming the configured result column binding into an expression tree.
/// </summary>
/// <typeparam name="TBuilder">The type of the result column builder.</typeparam>
/// <typeparam name="TDbDataReader">The type of the <see cref="DbDataReader"/> to be used.</typeparam>
/// <remarks>
/// Creates a new <see cref="ResultColumnCompiler{TBuilder, TDbDataReader}"/> instance.
/// </remarks>
/// <param name="builder">The result column builder.</param>
public abstract class ResultColumnCompiler<TBuilder, TDbDataReader>(TBuilder builder)
    where TBuilder : IResultColumnBuilder
    where TDbDataReader : DbDataReader
{
    /// <summary>
    /// The result column builder.
    /// </summary>
    protected TBuilder Builder { get; } = builder;

    /// <inheritdoc cref="IResultColumnCompiler.Compile"/>
    public virtual CompiledResultColumn Compile()
    {
        ParameterExpression readerExpression = Expression.Parameter(typeof(TDbDataReader), "reader");
        ConstantExpression nameExpression = Expression.Constant(Builder.ColumnName, typeof(string));

        IResultColumnCompilerHint? compilerSettings = Builder.CompilerHint;
        if (compilerSettings is null)
        {
            (Type readerResultType, Expression readerGetExpression) = GetColumnFactory();
            compilerSettings = new ResultColumnCompilerSettings
            (
                readerGetExpression,
                readerResultType,
                GetColumnConverterOrDefault()
            );
        }
        
        Expression valueExpression = Expression.Invoke(compilerSettings.ReaderGetExpression, readerExpression, nameExpression);

        // Handle null values
        if (Builder.IsNullable)
        {
            Expression<Func<TDbDataReader, string, bool>> checkDbNull = (reader, name) => reader.IsDBNull(reader.GetOrdinal(name));
            InvocationExpression checkDbNullExpression = Expression.Invoke(checkDbNull, readerExpression, nameExpression);
            ConstantExpression nullExpression;
            Expression notNullExpression;

            if (compilerSettings.ReaderResultType.IsValueType)
            {
                // value types need to be wrapped in a nullable
                nullExpression = Expression.Constant(default, typeof(Nullable<>)
                    .MakeGenericType(compilerSettings.ReaderResultType));
                notNullExpression = Expression.New(typeof(Nullable<>)
                    .MakeGenericType(compilerSettings.ReaderResultType)
                        .GetConstructor([compilerSettings.ReaderResultType])!, valueExpression);
            }
            else
            {
                nullExpression = Expression.Constant(null, compilerSettings.ReaderResultType);
                notNullExpression = valueExpression;
            }

            // this is a ternary <condition> ? <on true> : <on false>
            valueExpression = Expression.Condition(checkDbNullExpression, nullExpression, notNullExpression);
        }

        // we may need to wrap everything in a user-supplied or automatic conversion
        Expression? conversion = Builder.Conversion ?? compilerSettings.AutoConversion;
        if (conversion is not null)
        {
            valueExpression = Expression.Invoke(conversion, valueExpression);
        }
        Expression factory = Expression.Lambda(valueExpression, readerExpression);
        return new CompiledResultColumn(Builder.ColumnName!, Builder.Context.ResultProperty.Name, Builder.Context.ResultProperty.PropertyType, factory);
    }

    /// <summary>
    /// Automatically determines the best conversion expression for the column or returns <see langword="null"/> if no conversion is necessary or possible.
    /// </summary>
    protected abstract Expression? GetColumnConverterOrDefault();

    /// <summary>
    /// Gets the type of the column value and the expression to read the value from the <see cref="DbDataReader"/>.
    /// </summary>
    protected abstract (Type, Expression) GetColumnFactory();

    /// <summary>
    /// A helper method to create a tuple of the column type and the expression to read the value from the <see cref="DbDataReader"/>.
    /// </summary>
    /// <typeparam name="TResult">The type of the column value.</typeparam>
    /// <param name="expression">The expression to read the value from the <see cref="DbDataReader"/>.</param>
    protected static (Type, Expression<Func<TDbDataReader, string, TResult>>) ReadColumn<TResult>(Expression<Func<TDbDataReader, string, TResult>> expression) =>
        (typeof(TResult), expression);
}
