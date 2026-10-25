using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Jobs;
using System;
using System.Buffers.Binary;

namespace FSM_Serialization_Benchmarks;

/// <summary>
/// Measures decode-only cost for the exact value distributions used by the
/// integer-encoding benchmarks. Encoding and fixture creation happen in setup,
/// not in the timed operations.
/// </summary>
[MemoryDiagnoser]
[SimpleJob(RuntimeMoniker.Net80)]
[BenchmarkCategory("SER-INT")]
public class IntegerDecodingBenchmarks
{
    [Params("Small", "Mixed", "Large")]
    public string Distribution { get; set; } = "Small";

    private uint[] _expected = null!;
    private byte[] _fixedWidth = null!;
    private byte[] _uleb128 = null!;
    private int _uleb128Length;

    [GlobalSetup]
    public void Setup()
    {
        _expected = Distribution switch
        {
            "Small" => new uint[] { 0, 1, 2, 7, 15, 31, 63, 127 },
            "Mixed" => new uint[] { 0, 1, 127, 128, 255, 16_383, 16_384, 65_535, 1_000_000 },
            "Large" => new uint[] { 0, 127, 128, 16_383, 16_384, 1_000_000, 0x7FFF_FFFF, 0xFFFF_FFFF },
            _ => throw new InvalidOperationException($"Unknown distribution: {Distribution}")
        };

        _fixedWidth = new byte[checked(_expected.Length * sizeof(uint))];
        _uleb128 = new byte[checked(_expected.Length * 5)];

        for (int i = 0; i < _expected.Length; i++)
            BinaryPrimitives.WriteUInt32LittleEndian(
                _fixedWidth.AsSpan(i * sizeof(uint), sizeof(uint)), _expected[i]);

        foreach (uint value in _expected)
            _uleb128Length += WriteUleb128(value, _uleb128.AsSpan(_uleb128Length));

        // Validate fixtures before measuring; fixture generation is not timed.
        for (int i = 0; i < _expected.Length; i++)
        {
            uint decoded = BinaryPrimitives.ReadUInt32LittleEndian(
                _fixedWidth.AsSpan(i * sizeof(uint), sizeof(uint)));
            if (decoded != _expected[i])
                throw new InvalidOperationException($"Fixed-width decode mismatch at index {i}.");
        }

        int offset = 0;
        for (int i = 0; i < _expected.Length; i++)
        {
            uint decoded = ReadUleb128(_uleb128.AsSpan(offset, _uleb128Length - offset), out int consumed);
            if (decoded != _expected[i])
                throw new InvalidOperationException($"ULEB128 decode mismatch at index {i}.");
            offset += consumed;
        }

        if (offset != _uleb128Length)
            throw new InvalidOperationException("ULEB128 decoder did not consume the payload exactly.");

        AssertInvalidUleb128([0x80]); // truncated continuation
        AssertInvalidUleb128([0xFF, 0xFF, 0xFF, 0xFF, 0x10]); // UInt32 overflow
        AssertInvalidUleb128([0x80, 0x80, 0x80, 0x80, 0x80]); // too many continuation bytes
    }

    [Benchmark]
    public ulong SER_INT_003_FixedWidthUInt32_Decode()
    {
        ulong checksum = 0;
        for (int i = 0; i < _expected.Length; i++)
            checksum += BinaryPrimitives.ReadUInt32LittleEndian(
                _fixedWidth.AsSpan(i * sizeof(uint), sizeof(uint)));
        return checksum;
    }

    [Benchmark]
    public ulong SER_INT_004_Uleb128UInt32_Decode()
    {
        ulong checksum = 0;
        int offset = 0;
        while (offset < _uleb128Length)
        {
            checksum += ReadUleb128(_uleb128.AsSpan(offset, _uleb128Length - offset), out int consumed);
            offset += consumed;
        }
        return checksum;
    }

    private static void AssertInvalidUleb128(ReadOnlySpan<byte> encoded)
    {
        try
        {
            _ = ReadUleb128(encoded, out _);
        }
        catch (InvalidOperationException)
        {
            return;
        }

        throw new InvalidOperationException("ULEB128 decoder accepted malformed input.");
    }

    private static int WriteUleb128(uint value, Span<byte> destination)
    {
        int written = 0;
        while (value >= 0x80)
        {
            destination[written++] = (byte)((value & 0x7F) | 0x80);
            value >>= 7;
        }

        destination[written++] = (byte)value;
        return written;
    }

    private static uint ReadUleb128(ReadOnlySpan<byte> source, out int consumed)
    {
        uint value = 0;
        consumed = 0;

        for (int index = 0; index < 5; index++)
        {
            if (consumed >= source.Length)
                throw new InvalidOperationException("Truncated ULEB128 value.");

            byte current = source[consumed++];

            // A UInt32 has only four payload bits available in byte five.
            if (index == 4 && (current & 0xF0) != 0)
                throw new InvalidOperationException("ULEB128 value exceeds UInt32.");

            value |= (uint)(current & 0x7F) << (index * 7);

            if ((current & 0x80) == 0)
                return value;
        }

        throw new InvalidOperationException("Invalid ULEB128 value: too many continuation bytes.");
    }
}
