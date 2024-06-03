using System.Buffers.Binary;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json.Serialization;

namespace Wkg.EntityFrameworkCore.DataTypes;

/// <summary>
/// Represents a universally unique identifier (UUID) with big-endian byte order.
/// </summary>
[JsonConverter(typeof(UuidJsonConverter))]
[InlineArray(16)]
public struct Uuid : IEqualityOperators<Uuid, Uuid, bool>, IEquatable<Uuid>
{
    private byte _element0;

    /// <summary>
    /// Initializes a new instance of the <see cref="Uuid"/> structure from the provided <paramref name="guid"/>.
    /// </summary>
    /// <remarks>
    /// Byte order is preserved when converting between <see cref="Guid"/> and <see cref="Uuid"/>, string representation will differ.
    /// </remarks>
    /// <param name="guid">The <see cref="Guid"/> to create the <see cref="Uuid"/> from.</param>
    public Uuid(Guid guid)
    {
        Span<byte> self = AsSpan();
        guid.TryWriteBytes(self);
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="Uuid"/> structure from the provided bytes.
    /// </summary>
    /// <param name="b">The bytes to create the <see cref="Uuid"/> from.</param>
    public Uuid(ReadOnlySpan<byte> b)
    {
        if (b.Length != 16)
        {
            throw new ArgumentException("Invalid UUID byte length.", nameof(b));
        }
        Span<byte> self = AsSpan();
        b.CopyTo(self);
    }

    /// <summary>
    /// A read-only instance of the <see cref="Uuid"/> structure whose value is all zeros.
    /// </summary>
    public static Uuid Empty => default;

    /// <summary>
    /// Initializes a new instance of the <see cref="Uuid"/> structure from an underlying <see cref="Guid"/>.
    /// </summary>
    /// <returns>
    /// A new UUID object.
    /// </returns>
    public static Uuid NewUuid() => new(Guid.NewGuid());

    /// <summary>
    /// Returns a new instance of the <see cref="Uuid"/> structure with a version 4 UUID.
    /// </summary>
    public static Uuid NewUuidV4()
    {
        Uuid uuid = default;
        Span<byte> buffer = uuid.AsSpan();
        Random.Shared.NextBytes(buffer);
        NewUuidV4Core(buffer);
        return uuid;
    }

    /// <summary>
    /// Returns a new instance of the <see cref="Uuid"/> structure with a version 4 UUID using a cryptographically secure random number generator.
    /// </summary>
    public static Uuid NewUuidV4Secure()
    {
        Uuid uuid = default;
        Span<byte> buffer = uuid.AsSpan();
        RandomNumberGenerator.Fill(buffer);
        NewUuidV4Core(buffer);
        return uuid;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void NewUuidV4Core(Span<byte> buffer)
    {
        buffer[6] = (byte)(buffer[6] & 0x0F | 0x40);
        buffer[8] = (byte)(buffer[8] & 0x3F | 0x80);
    }

    /// <summary>
    /// Returns a new instance of the <see cref="Uuid"/> structure with a version 7 UUID.
    /// </summary>
    public static Uuid NewUuidV7()
    {
        Uuid uuid = default;
        Span<byte> buffer = uuid.AsSpan();
        Random.Shared.NextBytes(buffer);
        NewUuidV7Core(buffer);
        return uuid;
    }

    /// <summary>
    /// Returns a new instance of the <see cref="Uuid"/> structure with a version 7 UUID using a cryptographically secure random number generator.
    /// </summary>
    public static Uuid NewUuidV7Secure()
    {
        Uuid uuid = default;
        Span<byte> buffer = uuid.AsSpan();
        RandomNumberGenerator.Fill(buffer);
        NewUuidV7Core(buffer);
        return uuid;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void NewUuidV7Core(Span<byte> buffer)
    {
        ulong timestamp = (ulong)DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
        ulong timestamp48 = timestamp << 16;
        uint bytes6To9 = BinaryPrimitives.ReadUInt32LittleEndian(buffer[6..10]);
        BinaryPrimitives.WriteUInt64BigEndian(buffer, timestamp48);
        uint bytes6To9Masked = (bytes6To9 & 0x0FFF3FFF) | 0x70008000;
        BinaryPrimitives.WriteUInt32BigEndian(buffer[6..], bytes6To9Masked);
    }

    /// <summary>
    /// Gets the version of the UUID.
    /// </summary>
    /// <value>
    /// The version of the UUID. If the version is less than 1 or greater than 8, or if the variant is not RFC 4122, -1 is returned.
    /// </value>
    public readonly int Version
    {
        get
        {
            int version = this[6] >> 4;
            if (version is < 1 or > 8 || (this[8] & 0b1100_0000) != 0b1000_0000)
            {
                return -1;
            }
            return version;
        }
    }

    /// <summary>
    /// Tries to get the version of the UUID.
    /// </summary>
    /// <param name="version">When this method returns, contains the version of the UUID, if the conversion succeeded, or -1 if the version is not valid in accordance with RFC 4122.</param>
    /// <returns>
    /// <see langword="true"/> if the UUID version is valid; otherwise, <see langword="false"/>.
    /// </returns>
    public readonly bool TryGetVersion(out int version)
    {
        version = Version;
        return version != -1;
    }

    /// <inheritdoc cref="Guid.ToByteArray()" />
    public readonly byte[] GetBytes() => AsReadOnlySpan().ToArray();

    /// <summary>
    /// Copies the bytes of this UUID instance to the provided <paramref name="bytes"/>.
    /// </summary>
    /// <param name="bytes">The destination span to copy the bytes to.</param>
    public readonly void GetBytes(Span<byte> bytes) => AsReadOnlySpan().CopyTo(bytes);

    /// <summary>
    /// Returns this UUID instance as a 16-byte span.
    /// </summary>
    public Span<byte> AsSpan() => MemoryMarshal.CreateSpan(ref _element0, 16);

    /// <summary>
    /// Returns this UUID instance as a 16-byte read-only span.
    /// </summary>
    public readonly ReadOnlySpan<byte> AsReadOnlySpan() => MemoryMarshal.CreateReadOnlySpan(in _element0, 16);

    /// <inheritdoc/>
    public readonly bool Equals(Uuid other) => this == other;

    /// <summary>
    /// Returns a 36-character string that represents the current <see cref="Uuid"/> object.
    /// </summary>
    public override readonly string ToString()
    {
        // allocate enough bytes to store Uuid ASCII string
        Span<byte> result = stackalloc byte[36];

        // write Uuid to buffer
        ToStringCore(in this, result);

        // get string from ASCII encoded uuid byte array
        return Encoding.ASCII.GetString(result);
    }

    /// <summary>
    /// Writes this <see cref="Uuid"/> to the specified buffer in its big endian ASCII representation.
    /// </summary>
    /// <param name="buffer">The buffer to write the <see cref="Uuid"/> to, must be at least 36 bytes long.</param>
    public readonly void ToString(Span<byte> buffer) => ToStringCore(in this, buffer);

    private static void ToStringCore(ref readonly Uuid uuid, Span<byte> buffer)
    {
        ReadOnlySpan<byte> source = uuid.AsReadOnlySpan();
        int skip = 0;

        // iterate over uuid bytes
        for (int i = 0; i < source.Length; i++)
        {
            // indices 4, 6, 8 and 10 will contain a '-' delimiter character in the uuid string.
            // --> leave space for those delimiters
            // we can check if i is even and i / 2 is >= 2 and <= 5 to determine if we are at one of those indices
            // 0xF...F if i is odd and 0x0...0 if i is even
            int isOddMask = -(i & 1);

            // 0xF...F if i / 2 is < 2 and 0x0...0 if i / 2 is >= 2
            int less2Mask = (i >> 1) - 2 >> 31;

            // 0xF...F if i / 2 is > 5 and 0x0...0 if i / 2 is <= 5
            int greater5Mask = ~((i >> 1) - 6 >> 31);

            // 0xF...F if i is even and 2 <= i / 2 <= 5 otherwise 0x0...0
            int skipIndexMask = ~(isOddMask | less2Mask | greater5Mask);

            // skipIndexMask will be 0xFFFFFFFF for indices 4, 6, 8 and 10 and 0x00000000 for all other indices
            // --> skip those indices
            skip += 1 & skipIndexMask;
            buffer[2 * i + skip] = ToHexCharBranchless(source[i] >>> 0x4);
            buffer[2 * i + skip + 1] = ToHexCharBranchless(source[i] & 0x0F);
        }

        // add dashes
        const byte dash = (byte)'-';
        buffer[8] = buffer[13] = buffer[18] = buffer[23] = dash;
    }

    /// <summary>
    /// Converts the string representation of a UUID to its <see cref="Guid"/> equivalent (byte equivalence, string representation will differ).
    /// </summary>
    public readonly Guid ToGuid() => new(AsReadOnlySpan());

    /// <summary>
    /// Indicates whether the current <see cref="Uuid"/> object is empty (all bits are zero).
    /// </summary>
    public readonly bool IsEmpty => this == Empty;

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

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static byte ToHexCharBranchless(int b) =>
        // b + 0x30 for [0-9] if 0 <= b <= 9 and b + 0x30 + 0x27 for [a-f] if 10 <= b <= 15
        (byte)(b + 0x30 + (0x27 & ~(b - 0xA >> 31)));

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
    public static bool operator ==(Uuid left, Uuid right)
    {
        ReadOnlySpan<byte> leftBytes = left.AsReadOnlySpan();
        ReadOnlySpan<byte> rightBytes = right.AsReadOnlySpan();
        return leftBytes.SequenceEqual(rightBytes);
    }

    /// <inheritdoc/>
    public static bool operator !=(Uuid left, Uuid right) => !(left == right);

    /// <inheritdoc/>
    public readonly override bool Equals(object? obj) => obj is Uuid uuid && Equals(uuid);

    /// <inheritdoc/>
    public override int GetHashCode()
    {
        ulong hi = Unsafe.As<byte, ulong>(ref this[0]);
        ulong lo = Unsafe.As<byte, ulong>(ref this[8]);
        return HashCode.Combine(hi, lo);
    }
}
