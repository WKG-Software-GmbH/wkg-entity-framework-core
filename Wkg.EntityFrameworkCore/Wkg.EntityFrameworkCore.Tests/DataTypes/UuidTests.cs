using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Wkg.EntityFrameworkCore.DataTypes;

namespace Wkg.EntityFrameworkCore.Tests.DataTypes;

[TestClass]
public class UuidTests
{
    [TestMethod]
    public void ParseTest()
    {
        string uuid = "123e4567-e89b-12d3-a456-426614174000";
        Uuid result = Uuid.ParseUnsafe(uuid);
        // pointer cast to byte array
        ReadOnlySpan<byte> bytes = MemoryMarshal.CreateReadOnlySpan(ref Unsafe.As<Uuid, byte>(ref result), 16);
        Assert.AreEqual(uuid.Replace("-", ""), Convert.ToHexString(bytes).ToLowerInvariant());
    }

    [TestMethod]
    public void TryParseTest()
    {
        string uuid = "123e4567-e89b-12d3-a456-426614174000";
        Assert.IsTrue(Uuid.TryParse(uuid, out Uuid result));
        // pointer cast to byte array
        ReadOnlySpan<byte> bytes = MemoryMarshal.CreateReadOnlySpan(ref Unsafe.As<Uuid, byte>(ref result), 16);
        Assert.AreEqual(uuid.Replace("-", ""), Convert.ToHexString(bytes).ToLowerInvariant());

        // invalid length
        Assert.IsFalse(Uuid.TryParse("123e4567-e89b-12d3-a456-42661417400", out _));
        // invalid format
        Assert.IsFalse(Uuid.TryParse("123e4567-e89b-12d3-a456-4266141740g", out _));
        // invalid format
        Assert.IsFalse(Uuid.TryParse("123e4567-e89b-12d3-a456342661417400", out _));
        // invalid format
        Assert.IsFalse(Uuid.TryParse("123e4567-e89b-12d3-a456-4266141740-", out _));
    }

    [TestMethod]
    public void ToStringTest()
    {
        Span<byte> bytes = stackalloc byte[16];
        Random.Shared.NextBytes(bytes);
        char[] chars = [
            .. Convert.ToHexString(bytes[..4]),
            '-',
            .. Convert.ToHexString(bytes[4..6]),
            '-',
            .. Convert.ToHexString(bytes[6..8]),
            '-',
            .. Convert.ToHexString(bytes[8..10]),
            '-',
            .. Convert.ToHexString(bytes[10..])
        ];
        string expected = new string(chars).ToLowerInvariant();
        Uuid result = new(bytes);
        Assert.AreEqual(expected, result.ToString());
    }

    [TestMethod]
    public void ToStringIsInverseOfParseTest()
    {
        string uuid = "123e4567-e89b-12d3-a456-426614174000";
        Uuid result = Uuid.ParseUnsafe(uuid);
        Assert.AreEqual(uuid, result.ToString());
    }

    [TestMethod]
    public void NewUuidV4Test()
    {
        for (int i = 0; i < 1000; i++)
        {
            Uuid result = Uuid.NewUuidV4();
            Assert.IsTrue(result[6] >> 4 == 4);
            Assert.IsTrue((result[8] & 0b1100_0000) == 0b1000_0000);
        }
    }

    [TestMethod]
    public void NewUuidV7Test()
    {
        for (int i = 0; i < 1000; i++)
        {
            Uuid result = Uuid.NewUuidV7();
            Assert.IsTrue(result[6] >> 4 == 7);
            Assert.IsTrue((result[8] & 0b1100_0000) == 0b1000_0000);
        }
    }
}