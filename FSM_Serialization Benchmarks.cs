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

public sealed class ConstructionBenchmarks : FSMSerializationBenchmarkBase
{
    [Benchmark(Baseline = true)]
    public MemoryStream MemoryStream_Construct() => new();

    [Benchmark]
    public MemoryBinaryStream MemoryBinaryStream_Construct() => new();

    [Benchmark]
    public StreamBinaryStream StreamBinaryStream_Construct()
        => new(new MemoryStream());
}

public sealed class WriteBenchmarks : FSMSerializationBenchmarkBase
{
    [Benchmark(Baseline = true)]
    public MemoryStream MemoryStream_Write()
    {
        var stream = new MemoryStream();
        stream.Write(Data);
        return stream;
    }

    [Benchmark]
    public MemoryBinaryStream MemoryBinaryStream_Write()
    {
        var stream = new MemoryBinaryStream();
        stream.Write(Data);
        return stream;
    }

    [Benchmark]
    public StreamBinaryStream StreamBinaryStream_Write()
    {
        var stream = new StreamBinaryStream(new MemoryStream());
        stream.Write(Data);
        return stream;
    }
}

public sealed class ReadBenchmarks : FSMSerializationBenchmarkBase
{
    [Benchmark(Baseline = true)]
    public int MemoryStream_Read()
    {
        using var stream = new MemoryStream(Data, writable: false);
        return stream.Read(ReadBuffer);
    }

    [Benchmark]
    public int MemoryBinaryStream_Read()
    {
        using var stream = new MemoryBinaryStream(Data);
        return stream.Read(ReadBuffer);
    }

    [Benchmark]
    public int StreamBinaryStream_Read()
    {
        using var backing = new MemoryStream(Data, writable: false);
        using var stream = new StreamBinaryStream(backing);
        return stream.Read(ReadBuffer);
    }
}

public sealed class ExistingDataConstructionBenchmarks : FSMSerializationBenchmarkBase
{
    [Benchmark(Baseline = true)]
    public MemoryStream MemoryStream_ConstructWithData()
        => new(Data, writable: false);

    [Benchmark]
    public MemoryBinaryStream MemoryBinaryStream_ConstructWithData()
        => new(Data);
}

public sealed class ToArrayBenchmarks : FSMSerializationBenchmarkBase
{
    [Benchmark(Baseline = true)]
    public byte[] MemoryStream_ToArray()
    {
        using var stream = new MemoryStream();
        stream.Write(Data);
        return stream.ToArray();
    }

    [Benchmark]
    public byte[] MemoryBinaryStream_ToArray()
    {
        using var stream = new MemoryBinaryStream();
        stream.Write(Data);
        return stream.ToArray();
    }
}

public sealed class PositionOverwriteBenchmarks : FSMSerializationBenchmarkBase
{
    [Benchmark(Baseline = true)]
    public void MemoryStream_PositionOverwrite()
    {
        using var stream = new MemoryStream(Data.AsSpan().ToArray());
        stream.Position = Size / 2;
        stream.WriteByte(0x7F);
    }

    [Benchmark]
    public void MemoryBinaryStream_PositionOverwrite()
    {
        using var stream = new MemoryBinaryStream(Data.AsSpan().ToArray());
        stream.Position = Size / 2;
        stream.Write([0x7F]);
    }
}

public sealed class SerializationContractBenchmarks : FSMSerializationBenchmarkBase
{
    [Benchmark]
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
