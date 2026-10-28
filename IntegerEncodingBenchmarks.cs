using BenchmarkDotNet.Attributes;
using System;
using System.Buffers.Binary;

namespace FSM_Serialization_Benchmarks;

/// <summary>
/// Compares fixed-width little-endian UInt32 with canonical ULEB128 over identical values.
/// Correctness validation and fixture creation happen outside timed operations.
/// </summary>
[MemoryDiagnoser]
[BenchmarkCategory("SER-INT")]
public class IntegerEncodingBenchmarks
{
    [Params("Small", "Mixed", "Large")]
    public string Distribution { get; set; } = "Small";

    private uint[] _values = null!;

    [GlobalSetup]
    public void Setup()
    {
        _values = IntegerBenchmarkFixtures.Values(Distribution);
        byte[] fixedWidth = new byte[checked(_values.Length * sizeof(uint))];
        for (int i = 0; i < _values.Length; i++)
        {
            BinaryPrimitives.WriteUInt32LittleEndian(fixedWidth.AsSpan(i * sizeof(uint), sizeof(uint)), _values[i]);
            if (BinaryPrimitives.ReadUInt32LittleEndian(fixedWidth.AsSpan(i * sizeof(uint), sizeof(uint))) != _values[i])
                throw new InvalidOperationException($"Fixed-width round-trip mismatch at index {i}.");
        }

        byte[] variableWidth = new byte[checked(_values.Length * 5)];
        int offset = 0;
        foreach (uint value in _values)
            offset += IntegerBenchmarkFixtures.WriteUleb128(value, variableWidth.AsSpan(offset));

        int readOffset = 0;
        for (int i = 0; i < _values.Length; i++)
        {
            uint decoded = IntegerBenchmarkFixtures.ReadCanonicalUleb128(variableWidth.AsSpan(readOffset, offset - readOffset), out int consumed);
            if (decoded != _values[i])
                throw new InvalidOperationException($"ULEB128 round-trip mismatch at index {i}.");
            readOffset += consumed;
        }
        if (readOffset != offset)
            throw new InvalidOperationException("ULEB128 decoder did not consume the payload exactly.");
    }

    [Benchmark]
    public int SER_INT_001_FixedWidthUInt32()
    {
        byte[] output = new byte[checked(_values.Length * sizeof(uint))];
        for (int i = 0; i < _values.Length; i++)
            BinaryPrimitives.WriteUInt32LittleEndian(output.AsSpan(i * sizeof(uint), sizeof(uint)), _values[i]);
        return output.Length;
    }

    [Benchmark]
    public int SER_INT_002_Uleb128UInt32()
    {
        byte[] output = new byte[checked(_values.Length * 5)];
        int offset = 0;
        foreach (uint value in _values)
            offset += IntegerBenchmarkFixtures.WriteUleb128(value, output.AsSpan(offset));
        return offset;
    }
}

internal static class IntegerBenchmarkFixtures
{
    internal const int ValuesPerDistribution = 4096;

    internal static uint[] Values(string distribution)
    {
        uint[] pattern = distribution switch
        {
            "Small" => new uint[] { 0, 1, 2, 7, 15, 31, 63, 127 },
            "Mixed" => new uint[] { 0, 1, 127, 128, 255, 16_383, 16_384, 65_535, 1_000_000 },
            "Large" => new uint[] { 0, 127, 128, 16_383, 16_384, 1_000_000, 0x7FFF_FFFF, 0xFFFF_FFFF },
            _ => throw new InvalidOperationException($"Unknown distribution: {distribution}")
        };

        // Repeat the deterministic edge-rich pattern to amortize per-invocation overhead
        // while preserving the same value sequence for encode and decode comparisons.
        var values = new uint[ValuesPerDistribution];
        for (int i = 0; i < values.Length; i++)
            values[i] = pattern[i % pattern.Length];
        return values;
    }

    internal static int WriteUleb128(uint value, Span<byte> destination)
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

    internal static uint ReadCanonicalUleb128(ReadOnlySpan<byte> source, out int consumed)
    {
        uint value = 0;
        consumed = 0;
        for (int index = 0; index < 5; index++)
        {
            if (consumed >= source.Length)
                throw new InvalidOperationException("Truncated ULEB128 value.");

            byte current = source[consumed++];
            if (index == 4 && (current & 0xF0) != 0)
                throw new InvalidOperationException("ULEB128 value exceeds UInt32.");

            uint payload = (uint)(current & 0x7F);
            value |= payload << (index * 7);

            if ((current & 0x80) == 0)
            {
                if (index > 0 && payload == 0)
                    throw new InvalidOperationException("Non-canonical ULEB128 value: overlong encoding.");
                return value;
            }
        }
        throw new InvalidOperationException("Invalid ULEB128 value: too many continuation bytes.");
    }
}
