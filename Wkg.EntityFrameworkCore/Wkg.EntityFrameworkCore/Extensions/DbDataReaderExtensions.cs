using System.Data.Common;
using System.Runtime.CompilerServices;

namespace Wkg.EntityFrameworkCore.Extensions;

/// <summary>
/// Provides extension methods for <see cref="DbDataReader"/>.
/// </summary>
public static class DbDataReaderExtensions
{
    /// <summary>
    /// Gets the value of the specified column as a <see cref="byte"/>[].
    /// </summary>
    /// <param name="reader">The <see cref="DbDataReader"/> to get the value from.</param>
    /// <param name="ordinal">The zero-based column ordinal.</param>
    /// <returns>The value of the specified column.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static byte[] GetBytes(this DbDataReader reader, int ordinal)
    {
        using Stream stream = reader.GetStream(ordinal);
        using MemoryStream memoryStream = new();
        stream.CopyTo(memoryStream);
        return memoryStream.ToArray();
    }

    /// <summary>
    /// Gets the value of the specified column as a <see cref="byte"/>[].
    /// </summary>
    /// <param name="reader">The <see cref="DbDataReader"/> to get the value from.</param>
    /// <param name="name">The name of the column.</param>
    /// <returns>The value of the specified column.</returns>
    public static byte[] GetBytes(this DbDataReader reader, string name)
    {
        int ordinal = reader.GetOrdinal(name);
        return GetBytes(reader, ordinal);
    }
}