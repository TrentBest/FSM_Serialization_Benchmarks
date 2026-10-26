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
    [Benchmark(Description = "SER-RED-001 | MemoryStream_Read", Baseline = true)]
    public int MemoryStream_Read()
    {
        using var stream = new MemoryStream(Data, writable: false);
        return stream.Read(ReadBuffer);
    }

    [Benchmark(Description = "SER-RED-002 | MemoryBinaryStream_Read")]
    public int MemoryBinaryStream_Read()
    {
        using var stream = new MemoryBinaryStream(Data);
        return stream.Read(ReadBuffer);
    }

    [Benchmark(Description = "SER-RED-003 | StreamBinaryStream_Read")]
    public int StreamBinaryStream_Read()
    {
        using var backing = new MemoryStream(Data, writable: false);
        using var stream = new StreamBinaryStream(backing);
        return stream.Read(ReadBuffer);
    }
}

public class ExistingDataConstructionBenchmarks : FSMSerializationBenchmarkBase
{
    [Benchmark(Description = "SER-DAT-001 | MemoryStream_ConstructWithData", Baseline = true)]
    public MemoryStream MemoryStream_ConstructWithData()
        => new(Data, writable: false);

    [Benchmark(Description = "SER-DAT-002 | MemoryBinaryStream_ConstructWithData")]
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
