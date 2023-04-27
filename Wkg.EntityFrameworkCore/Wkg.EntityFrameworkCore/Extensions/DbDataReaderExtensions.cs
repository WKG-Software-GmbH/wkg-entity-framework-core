using System.Data.Common;
using System.Runtime.CompilerServices;

namespace Wkg.EntityFrameworkCore.Extensions;

public static class DbDataReaderExtensions
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static byte[] GetBytes(this DbDataReader reader, int ordinal)
    {
        using Stream stream = reader.GetStream(ordinal);
        using MemoryStream memoryStream = new();
        stream.CopyTo(memoryStream);
        return memoryStream.ToArray();
    }

    public static byte[] GetBytes(this DbDataReader reader, string name)
    {
        int ordinal = reader.GetOrdinal(name);
        return GetBytes(reader, ordinal);
    }
}