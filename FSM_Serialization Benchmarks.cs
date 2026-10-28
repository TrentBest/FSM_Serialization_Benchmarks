using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Jobs;
using System;
using System.Buffers.Binary;
using System.IO;
using TheSingularityWorkshop.FSM_Serialization;

namespace FSM_Serialization_Benchmarks;

[MemoryDiagnoser]
[SimpleJob(RuntimeMoniker.Net80)]
public abstract class FSMSerializationBenchmarkBase
{
    [Params(16, 1024, 10_000, 100_000)]
    public int Size { get; set; }

    protected byte[] Data = null!;
    protected byte[] ReadBuffer = null!;

    [GlobalSetup]
    public void Setup()
    {
        Data = new byte[Size];
        ReadBuffer = new byte[Size];

        for (int i = 0; i < Data.Length; i++)
            Data[i] = (byte)(i * 31);
    }
}

public class ConstructionBenchmarks : FSMSerializationBenchmarkBase
{
    [Benchmark(Description = "SER-STR-001 | MemoryStream_Construct", Baseline = true)]
    public MemoryStream MemoryStream_Construct() => new();

    [Benchmark(Description = "SER-STR-002 | MemoryBinaryStream_Construct")]
    public MemoryBinaryStream MemoryBinaryStream_Construct() => new();

    [Benchmark(Description = "SER-STR-003 | StreamBinaryStream_Construct")]
    public StreamBinaryStream StreamBinaryStream_Construct()
        => new(new MemoryStream());
}

public class WriteBenchmarks : FSMSerializationBenchmarkBase
{
    [Benchmark(Description = "SER-WRT-001 | MemoryStream_Write", Baseline = true)]
    public MemoryStream MemoryStream_Write()
    {
        var stream = new MemoryStream();
        stream.Write(Data);
        return stream;
    }

    [Benchmark(Description = "SER-WRT-002 | MemoryBinaryStream_Write")]
    public MemoryBinaryStream MemoryBinaryStream_Write()
    {
        var stream = new MemoryBinaryStream();
        stream.Write(Data);
        return stream;
    }

    [Benchmark(Description = "SER-WRT-003 | StreamBinaryStream_Write")]
    public StreamBinaryStream StreamBinaryStream_Write()
    {
        var stream = new StreamBinaryStream(new MemoryStream());
        stream.Write(Data);
        return stream;
    }
}

public class ReadBenchmarks : FSMSerializationBenchmarkBase
{
    [Benchmark(Description = "SER-RED-001 | MemoryStream_ConstructAndRead", Baseline = true)]
    public int MemoryStream_Read()
    {
        using var stream = new MemoryStream(Data, writable: false);
        return stream.Read(ReadBuffer);
    }

    [Benchmark(Description = "SER-RED-002 | MemoryBinaryStream_ConstructAndRead")]
    public int MemoryBinaryStream_Read()
    {
        using var stream = new MemoryBinaryStream(Data);
        return stream.Read(ReadBuffer);
    }

    [Benchmark(Description = "SER-RED-003 | StreamBinaryStream_ConstructAndRead")]
    public int StreamBinaryStream_Read()
    {
        using var backing = new MemoryStream(Data, writable: false);
        using var stream = new StreamBinaryStream(backing);
        return stream.Read(ReadBuffer);
    }
}

/// <summary>
/// Measures read-call cost separately from stream construction and payload copying.
/// Both streams are prepared before measurement, with equivalent payload contents.
/// Position reset is included equally in both methods so each invocation reads the full payload.
/// </summary>
[MemoryDiagnoser]
[SimpleJob(RuntimeMoniker.Net80)]
public class ReadOperationBenchmarks
{
    [Params(16, 1024, 10_000, 100_000)]
    public int Size { get; set; }

    private byte[] _data = null!;
    private byte[] _readBuffer = null!;
    private MemoryStream _baseline = null!;
    private MemoryBinaryStream _candidate = null!;

    [GlobalSetup]
    public void Setup()
    {
        _data = new byte[Size];
        _readBuffer = new byte[Size];

        for (int i = 0; i < _data.Length; i++)
            _data[i] = (byte)(i * 31);

        // Prepare both streams outside the timed operation. The baseline owns its
        // independent payload array; MemoryBinaryStream copies the same payload.
        _baseline = new MemoryStream((byte[])_data.Clone(), writable: false);
        _candidate = new MemoryBinaryStream(_data);
    }

    [GlobalCleanup]
    public void Cleanup()
    {
        _baseline.Dispose();
        _candidate.Dispose();
    }

    [Benchmark(Description = "SER-RED-004 | MemoryStream_ReadPreparedPayload", Baseline = true)]
    public int MemoryStream_ReadPreparedPayload()
    {
        _baseline.Position = 0;
        return _baseline.Read(_readBuffer);
    }

    [Benchmark(Description = "SER-RED-005 | MemoryBinaryStream_ReadPreparedPayload")]
    public int MemoryBinaryStream_ReadPreparedPayload()
    {
        _candidate.Position = 0;
        return _candidate.Read(_readBuffer);
    }
}

public class ExistingDataConstructionBenchmarks : FSMSerializationBenchmarkBase
{
    [Benchmark(Description = "SER-DAT-001 | MemoryStream_WrapExistingData", Baseline = true)]
    public MemoryStream MemoryStream_ConstructWithData()
        => new(Data, writable: false);

    [Benchmark(Description = "SER-DAT-002 | MemoryBinaryStream_CopyExistingData")]
    public MemoryBinaryStream MemoryBinaryStream_ConstructWithData()
        => new(Data);
}

public class ToArrayBenchmarks : FSMSerializationBenchmarkBase
{
    [Benchmark(Description = "SER-ARR-001 | MemoryStream_ToArray", Baseline = true)]
    public byte[] MemoryStream_ToArray()
    {
        using var stream = new MemoryStream();
        stream.Write(Data);
        return stream.ToArray();
    }

    [Benchmark(Description = "SER-ARR-002 | MemoryBinaryStream_ToArray")]
    public byte[] MemoryBinaryStream_ToArray()
    {
        using var stream = new MemoryBinaryStream();
        stream.Write(Data);
        return stream.ToArray();
    }
}

public class PositionOverwriteBenchmarks : FSMSerializationBenchmarkBase
{
    [Benchmark(Description = "SER-POS-001 | MemoryStream_PositionOverwrite", Baseline = true)]
    public void MemoryStream_PositionOverwrite()
    {
        using var stream = new MemoryStream(Data.AsSpan().ToArray());
        stream.Position = Size / 2;
        stream.WriteByte(0x7F);
    }

    [Benchmark(Description = "SER-POS-002 | MemoryBinaryStream_PositionOverwrite")]
    public void MemoryBinaryStream_PositionOverwrite()
    {
        using var stream = new MemoryBinaryStream(Data.AsSpan().ToArray());
        stream.Position = Size / 2;
        stream.Write([0x7F]);
    }
}

public class SerializationContractBenchmarks : FSMSerializationBenchmarkBase
{
    [GlobalSetup(Target = nameof(BinarySerializable_RoundTrip))]
    public void ValidateRoundTripCorrectness()
    {
        var value = new BenchmarkValue(Data);

        using var stream = new MemoryBinaryStream();
        value.Pack(stream);
        stream.Position = 0;

        var restored = new BenchmarkValue(Size);
        restored.Unpack(stream);

        if (restored.Payload.Length != Data.Length)
            throw new InvalidOperationException("Round-trip payload length changed.");

        for (int i = 0; i < Data.Length; i++)
        {
            if (restored.Payload[i] != Data[i])
                throw new InvalidOperationException($"Round-trip payload differs at byte index {i}.");
        }

        if (stream.Position != stream.Length)
            throw new InvalidOperationException("Round-trip left trailing or unread bytes.");
    }

    [Benchmark(Description = "SER-RT-001 | BinarySerializable_RoundTrip")]
    public int BinarySerializable_RoundTrip()
    {
        var value = new BenchmarkValue(Data);

        using var stream = new MemoryBinaryStream();
        value.Pack(stream);

        stream.Position = 0;

        var restored = new BenchmarkValue(Size);
        restored.Unpack(stream);

        return restored.Payload.Length;
    }

    private sealed class BenchmarkValue : IBinarySerializable
    {
        public BenchmarkValue(byte[] payload) => Payload = payload;

        public BenchmarkValue(int payloadLength)
            => Payload = new byte[payloadLength];

        public byte[] Payload { get; private set; }

        public void Pack(IBinaryStream stream)
        {
            Span<byte> length = stackalloc byte[sizeof(int)];
            BinaryPrimitives.WriteInt32LittleEndian(length, Payload.Length);
            stream.Write(length);
            stream.Write(Payload);
        }

        public void Unpack(IBinaryStream stream)
        {
            Span<byte> length = stackalloc byte[sizeof(int)];

            if (stream.Read(length) != length.Length)
                throw new EndOfStreamException();

            int payloadLength = BinaryPrimitives.ReadInt32LittleEndian(length);
            Payload = new byte[payloadLength];

            if (stream.Read(Payload) != payloadLength)
                throw new EndOfStreamException();
        }
    }
}
