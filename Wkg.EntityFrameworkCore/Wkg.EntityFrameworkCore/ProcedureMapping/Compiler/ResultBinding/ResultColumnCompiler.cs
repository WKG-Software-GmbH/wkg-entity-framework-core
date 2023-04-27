using System.Data.Common;
using System.Linq.Expressions;
using Wkg.EntityFrameworkCore.ProcedureMapping.Builder.ResultBinding;
using Wkg.EntityFrameworkCore.ProcedureMapping.Compiler.Output;

namespace Wkg.EntityFrameworkCore.ProcedureMapping.Compiler.ResultBinding;

public interface IResultColumnCompiler
{
    CompiledResultColumn Compile();
}

public abstract class ResultColumnCompiler<TBuilder, TDbDataReader> 
    where TBuilder : IResultColumnBuilder
    where TDbDataReader : DbDataReader
{
    protected TBuilder Builder { get; }

    protected ResultColumnCompiler(TBuilder builder) => Builder = builder;

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
                        .GetConstructor(new[] { compilerSettings.ReaderResultType })!, valueExpression);
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

    protected abstract Expression? GetColumnConverterOrDefault();

    protected abstract (Type, Expression) GetColumnFactory();

    protected static (Type, Expression<Func<TDbDataReader, string, TResult>>) ReadColumn<TResult>(Expression<Func<TDbDataReader, string, TResult>> expression) =>
        (typeof(TResult), expression);
}
