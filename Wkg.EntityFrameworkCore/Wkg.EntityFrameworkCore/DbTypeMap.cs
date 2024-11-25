using System.Collections.Frozen;

namespace Wkg.EntityFrameworkCore;

/// <summary>
/// A base class for mapping CLR types to database types.
/// </summary>
/// <typeparam name="TDbType">The database type.</typeparam>
public abstract class DbTypeMap<TDbType> where TDbType : Enum
{
    /// <summary>
    /// A dictionary mapping CLR types to database types.
    /// </summary>
    protected abstract FrozenDictionary<Type, TDbType> TypeMap { get; }

    /// <summary>
    /// Gets the database type for the given CLR type or <see langword="null"/> if no known mapping exists.
    /// </summary>
    /// <param name="type">The CLR type.</param>
    /// <returns>The database type or <see langword="null"/> if no known mapping exists.</returns>
    public TDbType? GetDbTypeOrDefault(Type type)
    {
        if (Nullable.GetUnderlyingType(type) is Type underlyingType)
        {
            return GetDbTypeOrDefault(underlyingType);
        }
        if (type.IsEnum)
        {
            return GetDbTypeOrDefault(Enum.GetUnderlyingType(type));
        }
        return TypeMap.GetValueOrDefault(type);
    }
}