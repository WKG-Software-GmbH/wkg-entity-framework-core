using System.Diagnostics;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.Json.Serialization;
using Wkg.Common.Extensions;

namespace Wkg.EntityFrameworkCore.DataTypes.Uuids;

/// <summary>
/// Represents a universally unique identifier (UUID) with big-endian byte order.
/// </summary>
[StructLayout(LayoutKind.Explicit, Size = 16)]
[DebuggerDisplay("{ToString(),nq}")]
[JsonConverter(typeof(UuidJsonConverter))]
public readonly struct Uuid : IEqualityOperators<Uuid, Uuid, bool>, IEquatable<Uuid>
{
    [FieldOffset(0)]
    private readonly Guid _value;

    /// <summary>
    /// Initializes a new instance of the <see cref="Uuid"/> structure from the provided <paramref name="guid"/>.
    /// </summary>
    /// <remarks>
    /// Byte order is preserved when converting between <see cref="Guid"/> and <see cref="Uuid"/>, string representation will differ.
    /// </remarks>
    /// <param name="guid">The <see cref="Guid"/> to create the <see cref="Uuid"/> from.</param>
    public Uuid(Guid guid) => _value = guid;

    /// <summary>
    /// Initializes a new instance of the <see cref="Uuid"/> structure from the provided bytes.
    /// </summary>
    /// <param name="b">The bytes to create the <see cref="Uuid"/> from.</param>
    public Uuid(ReadOnlySpan<byte> b)
    {
        _value = new Guid(b);
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="Uuid"/> structure
    /// </summary>
    /// <returns>
    /// A new UUID object.
    /// </returns>
    public static Uuid NewUuid() => new(Guid.NewGuid());

    /// <inheritdoc/>
    public bool Equals(Uuid other) => this == other;

    /// <summary>
    /// Returns a 36-character string that represents the current <see cref="Uuid"/> object.
    /// </summary>
    public override string ToString() => _value.ToStringBigEndian();

    /// <summary>
    /// Converts the string representation of a UUID to its <see cref="Guid"/> equivalent (byte equivalence, string representation will differ).
    /// </summary>
    public Guid ToGuid() => _value;

    /// <summary>
    /// Indicates whether the current <see cref="Uuid"/> object is empty (all bits are zero).
    /// </summary>
    public bool IsEmpty => _value == Guid.Empty;

    /// <summary>
    /// Validates and parses the specified 36-character string representation of a UUID into a new <see cref="Uuid"/> object.
    /// </summary>
    /// <param name="s">The string to parse.</param>
    /// <returns>The resulting <see cref="Uuid"/> object.</returns>
    /// <exception cref="FormatException">The string is not in a valid UUID format.</exception>
    public static Uuid Parse(string s)
    {
        if (TryParse(s, out Uuid result))
        {
            return result;
        }
        throw new FormatException("Invalid UUID format.");
    }

    /// <summary>
    /// Parses the specified 36-character string representation of a UUID into a new <see cref="Uuid"/> object with minimal validation.
    /// </summary>
    /// <param name="s">The string to parse.</param>
    /// <returns>The resulting <see cref="Uuid"/> object.</returns>
    public static Uuid ParseUnsafe(ReadOnlySpan<char> s)
    {
        if (TryParseUnsafe(s, out Uuid result))
        {
            return result;
        }
        throw new FormatException("Invalid UUID format.");
    }

    /// <summary>
    /// Validates and attempts to parse the specified 36-character string representation of a UUID into a new <see cref="Uuid"/> object.
    /// </summary>
    /// <param name="s">The string to parse.</param>
    /// <param name="uuid">The resulting <see cref="Uuid"/> object.</param>
    /// <returns><see langword="true"/> if the string was successfully parsed; otherwise, <see langword="false"/>.</returns>    
    public static bool TryParse(string s, out Uuid uuid)
    {
        if (!IsValid(s))
        {
            uuid = default;
            return false;
        }
        return TryParseUnsafe(s, out uuid);
    }

    /// <summary>
    /// Attempts to parse the specified 36-character string representation of a UUID into a new <see cref="Uuid"/> object with minimal validation.
    /// </summary>
    /// <param name="s">The string to parse.</param>
    /// <param name="uuid">The resulting <see cref="Uuid"/> object.</param>
    /// <returns><see langword="true"/> if the string was successfully parsed; otherwise, <see langword="false"/>.</returns>
    public static bool TryParseUnsafe(ReadOnlySpan<char> s, out Uuid uuid)
    {
        const int SKIP_MAP = 1 << 8 | 1 << 13 | 1 << 18 | 1 << 23;

        if (Encoding.ASCII.GetByteCount(s) != 36)
        {
            uuid = default;
            return false;
        }
        Span<byte> bytes = stackalloc byte[36];
        Span<byte> result = stackalloc byte[16];
        Encoding.ASCII.GetBytes(s, bytes);
        int skip = 0;
        for (int i = 0; i < result.Length; i++)
        {
            int skipMask = -(1 << 2 * i + skip & SKIP_MAP) >> 31;
            skip += 1 & skipMask;
            int idx = 2 * i + skip;
            int hi = UnhexlifyAsciiNibble(bytes[idx]);
            int lo = UnhexlifyAsciiNibble(bytes[idx + 1]);
            result[i] = (byte)(hi << 4 | lo);
        }
        uuid = new Uuid(result);
        return true;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static int UnhexlifyAsciiNibble(int ascii) => (ascii & 0xF) + (ascii >> 6) | ascii >> 3 & 0x8;

    /// <summary>
    /// Validates the specified 36-character string representation of a UUID.
    /// </summary>
    /// <param name="s">The string to validate.</param>
    /// <returns><see langword="true"/> if the string is a valid UUID; otherwise, <see langword="false"/>.</returns>
    public static bool IsValid(ReadOnlySpan<char> s)
    {
        if (s.Length != 36)
        {
            return false;
        }
        for (int i = 0; i < s.Length; i++)
        {
            if (i is 8 or 13 or 18 or 23)
            {
                if (s[i] != '-')
                {
                    return false;
                }
            }
            else if (!char.IsAsciiHexDigit(s[i]))
            {
                return false;
            }
        }
        return true;
    }

    /// <inheritdoc/>
    public static bool operator ==(Uuid left, Uuid right) => left._value == right._value;

    /// <inheritdoc/>
    public static bool operator !=(Uuid left, Uuid right) => left._value != right._value;

    /// <inheritdoc/>
    public override bool Equals(object? obj) => obj is Uuid uuid && Equals(uuid);

    /// <inheritdoc/>
    public override int GetHashCode() => _value.GetHashCode();
}
