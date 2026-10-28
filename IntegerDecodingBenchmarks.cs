using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Jobs;
using System;
using System.Buffers.Binary;

namespace FSM_Serialization_Benchmarks;

/// <summary>
/// Measures decode-only cost. Payload creation and correctness checks are setup-only.
/// The local ULEB128 contract requires shortest-form (canonical) encodings.
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
        _expected = IntegerBenchmarkFixtures.Values(Distribution);
        _fixedWidth = new byte[checked(_expected.Length * sizeof(uint))];
        _uleb128 = new byte[checked(_expected.Length * 5)];

        for (int i = 0; i < _expected.Length; i++)
            BinaryPrimitives.WriteUInt32LittleEndian(_fixedWidth.AsSpan(i * sizeof(uint), sizeof(uint)), _expected[i]);

        foreach (uint value in _expected)
            _uleb128Length += IntegerBenchmarkFixtures.WriteUleb128(value, _uleb128.AsSpan(_uleb128Length));

        for (int i = 0; i < _expected.Length; i++)
        {
            uint decoded = BinaryPrimitives.ReadUInt32LittleEndian(_fixedWidth.AsSpan(i * sizeof(uint), sizeof(uint)));
            if (decoded != _expected[i])
                throw new InvalidOperationException($"Fixed-width decode mismatch at index {i}.");
        }

        int offset = 0;
        for (int i = 0; i < _expected.Length; i++)
        {
            uint decoded = IntegerBenchmarkFixtures.ReadCanonicalUleb128(_uleb128.AsSpan(offset, _uleb128Length - offset), out int consumed);
            if (decoded != _expected[i])
                throw new InvalidOperationException($"ULEB128 decode mismatch at index {i}.");
            offset += consumed;
        }
        if (offset != _uleb128Length)
            throw new InvalidOperationException("ULEB128 decoder did not consume the payload exactly.");

        AssertInvalid([0x80]); // truncated continuation
        AssertInvalid([0xFF, 0xFF, 0xFF, 0xFF, 0x10]); // UInt32 overflow
        AssertInvalid([0x80, 0x80, 0x80, 0x80, 0x80]); // too many continuation bytes
        AssertInvalid([0x80, 0x00]); // overlong/non-canonical zero
        AssertInvalid([0x81, 0x00]); // overlong/non-canonical one
    }

    [Benchmark]
    public ulong SER_INT_003_FixedWidthUInt32_Decode()
    {
        ulong checksum = 0;
        for (int i = 0; i < _expected.Length; i++)
            checksum += BinaryPrimitives.ReadUInt32LittleEndian(_fixedWidth.AsSpan(i * sizeof(uint), sizeof(uint)));
        return checksum;
    }

    [Benchmark]
    public ulong SER_INT_004_Uleb128UInt32_Decode()
    {
        ulong checksum = 0;
        int offset = 0;
        while (offset < _uleb128Length)
        {
            checksum += IntegerBenchmarkFixtures.ReadCanonicalUleb128(_uleb128.AsSpan(offset, _uleb128Length - offset), out int consumed);
            offset += consumed;
        }
        return checksum;
    }

    private static void AssertInvalid(ReadOnlySpan<byte> encoded)
    {
        try
        {
            _ = IntegerBenchmarkFixtures.ReadCanonicalUleb128(encoded, out _);
        }
        catch (InvalidOperationException)
        {
            return;
        }
        throw new InvalidOperationException("ULEB128 decoder accepted malformed or non-canonical input.");
    }
}
