using System;
using System.IO;
using System.IO.Compression;
using BenchmarkDotNet.Attributes;

namespace FSM_Serialization_Benchmarks;

/// <summary>
/// Measures independent per-section Brotli work for Experience resource/artifact
/// payload shapes. Fixture generation, compression correctness, and ratio calculation
/// are setup-time operations; timed methods measure only compression or decompression.
/// </summary>
[MemoryDiagnoser]
[BenchmarkCategory("SER-EXP")]
public class ExperienceBrotliBenchmarks
{
    [Params(1_024, 16_384, 262_144, 1_048_576)]
    public int Size { get; set; }

    [Params("Repetitive", "Structured", "HighEntropy")]
    public string PayloadKind { get; set; } = "Repetitive";

    [Params(CompressionLevel.Fastest, CompressionLevel.Optimal)]
    public CompressionLevel Level { get; set; }

    private byte[] _payload = null!;
    private byte[] _compressed = null!;

    [GlobalSetup]
    public void Setup()
    {
        _payload = CreatePayload(Size, PayloadKind);
        _compressed = Compress(_payload, Level);

        using var input = new MemoryStream(_compressed, writable: false);
        using var decoder = new BrotliStream(input, CompressionMode.Decompress);
        using var output = new MemoryStream();
        decoder.CopyTo(output);

        byte[] restored = output.ToArray();
        if (!restored.AsSpan().SequenceEqual(_payload))
            throw new InvalidOperationException("Brotli fixture failed byte-for-byte round-trip validation.");

        // A second independent compression must produce the same bytes for this
        // runtime/level/fixture. This checks repeatability within the measured environment.
        double ratio = (double)_compressed.Length / _payload.Length;
        Console.WriteLine($"SER-EXP-METRIC|size={Size}|kind={PayloadKind}|level={Level}|logicalBytes={_payload.Length}|storedBytes={_compressed.Length}|ratio={ratio:F6}");

        byte[] repeated = Compress(_payload, Level);
        if (!repeated.AsSpan().SequenceEqual(_compressed))
            throw new InvalidOperationException("Brotli output was not repeatable within the benchmark environment.");
    }

    /// <summary>Compression plus output-stream allocation; returns stored byte count.</summary>
    [Benchmark(Description = "SER-EXP-001 | Brotli_CompressSection")]
    public int Brotli_CompressSection()
        => Compress(_payload, Level).Length;

    /// <summary>Decompression of a prepared stored section; returns logical byte count.</summary>
    [Benchmark(Description = "SER-EXP-002 | Brotli_DecompressSection")]
    public int Brotli_DecompressSection()
    {
        using var input = new MemoryStream(_compressed, writable: false);
        using var decoder = new BrotliStream(input, CompressionMode.Decompress);
        using var output = new MemoryStream(_payload.Length);
        decoder.CopyTo(output);
        return checked((int)output.Length);
    }

    private static byte[] Compress(byte[] payload, CompressionLevel level)
    {
        using var output = new MemoryStream();
        using (var encoder = new BrotliStream(output, level, leaveOpen: true))
            encoder.Write(payload);
        return output.ToArray();
    }

    private static byte[] CreatePayload(int size, string kind)
    {
        var data = new byte[size];
        switch (kind)
        {
            case "Repetitive":
                Array.Fill(data, (byte)'A');
                break;

            case "Structured":
                // A deterministic, text/resource-like mixture with repeated records,
                // small numeric fields, delimiters, and bounded variation.
                ReadOnlySpan<byte> record = "bundle:physics/water;version:1.2.3;cell:0000;state:active\n"u8;
                for (int i = 0; i < data.Length; i++)
                {
                    int recordOffset = i % record.Length;
                    data[i] = record[recordOffset];
                    if (recordOffset >= 25 && recordOffset <= 28)
                        data[i] = (byte)('0' + ((i / record.Length + recordOffset) % 10));
                }
                break;

            case "HighEntropy":
                // Fixed-seed xorshift stream: deterministic, binary-like, and not
                // dependent on external randomness or wall-clock state.
                uint state = 0x9E3779B9;
                for (int i = 0; i < data.Length; i++)
                {
                    state ^= state << 13;
                    state ^= state >> 17;
                    state ^= state << 5;
                    data[i] = (byte)state;
                }
                break;

            default:
                throw new ArgumentOutOfRangeException(nameof(kind), kind, "Unknown payload fixture.");
        }

        return data;
    }
}
