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
    [Benchmark(Baseline = true)]
    public MemoryStream SER_STR_001_MemoryStream_Construct() => new();

    [Benchmark]
    public MemoryBinaryStream SER_STR_002_MemoryBinaryStream_Construct() => new();

    [Benchmark]
    public StreamBinaryStream SER_STR_003_StreamBinaryStream_Construct()
        => new(new MemoryStream());
}

public class WriteBenchmarks : FSMSerializationBenchmarkBase
{
    [Benchmark(Baseline = true)]
    public MemoryStream SER_WRT_001_MemoryStream_Write()
    {
        var stream = new MemoryStream();
        stream.Write(Data);
        return stream;
    }

    [Benchmark]
    public MemoryBinaryStream SER_WRT_002_MemoryBinaryStream_Write()
    {
        var stream = new MemoryBinaryStream();
        stream.Write(Data);
        return stream;
    }

    [Benchmark]
    public StreamBinaryStream SER_WRT_003_StreamBinaryStream_Write()
    {
        var stream = new StreamBinaryStream(new MemoryStream());
        stream.Write(Data);
        return stream;
    }
}

public class ReadBenchmarks : FSMSerializationBenchmarkBase
{
    [Benchmark(Baseline = true)]
    public int SER_RD_001_MemoryStream_Read()
    {
        using var stream = new MemoryStream(Data, writable: false);
        return stream.Read(ReadBuffer);
    }

    [Benchmark]
    public int SER_RD_002_MemoryBinaryStream_Read()
    {
        using var stream = new MemoryBinaryStream(Data);
        return stream.Read(ReadBuffer);
    }

    [Benchmark]
    public int SER_RD_003_StreamBinaryStream_Read()
    {
        using var backing = new MemoryStream(Data, writable: false);
        using var stream = new StreamBinaryStream(backing);
        return stream.Read(ReadBuffer);
    }
}

public class ExistingDataConstructionBenchmarks : FSMSerializationBenchmarkBase
{
    [Benchmark(Baseline = true)]
    public MemoryStream SER_CTR_001_MemoryStream_ConstructWithData()
        => new(Data, writable: false);

    [Benchmark]
    public MemoryBinaryStream SER_CTR_002_MemoryBinaryStream_ConstructWithData()
        => new(Data);
}

public class ToArrayBenchmarks : FSMSerializationBenchmarkBase
{
    [Benchmark(Baseline = true)]
    public byte[] SER_ARR_001_MemoryStream_ToArray()
    {
        using var stream = new MemoryStream();
        stream.Write(Data);
        return stream.ToArray();
    }

    [Benchmark]
    public byte[] SER_ARR_002_MemoryBinaryStream_ToArray()
    {
        using var stream = new MemoryBinaryStream();
        stream.Write(Data);
        return stream.ToArray();
    }
}

public class PositionOverwriteBenchmarks : FSMSerializationBenchmarkBase
{
    [Benchmark(Baseline = true)]
    public void SER_POS_001_MemoryStream_PositionOverwrite()
    {
        using var stream = new MemoryStream(Data.AsSpan().ToArray());
        stream.Position = Size / 2;
        stream.WriteByte(0x7F);
    }

    [Benchmark]
    public void SER_POS_002_MemoryBinaryStream_PositionOverwrite()
    {
        using var stream = new MemoryBinaryStream(Data.AsSpan().ToArray());
        stream.Position = Size / 2;
        stream.Write([0x7F]);
    }
}

public class SerializationContractBenchmarks : FSMSerializationBenchmarkBase
{
    [Benchmark]
    public int SER_RT_001_BinarySerializable_RoundTrip()
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


/// <summary>
/// Stable benchmark IDs are documented in docs/BENCHMARK_CATALOG.md.
/// This class isolates the output-buffer strategy while preserving the same
/// byte-for-byte payload and reporting the resulting payload length.
/// </summary>
[MemoryDiagnoser]
[SimpleJob(RuntimeMoniker.Net80)]
[BenchmarkCategory("SER-BUF")]
public class OutputBufferStrategyBenchmarks
{
    [Params(16, 1024, 10_000, 100_000)]
    public int Size { get; set; }

    private byte[] _data = null!;

    [GlobalSetup]
    public void Setup()
    {
        _data = new byte[Size];
        for (int i = 0; i < _data.Length; i++)
            _data[i] = (byte)(i * 31);
    }

    [Benchmark]
    public int SER_BUF_001_MemoryStream_Grow()
    {
        using var stream = new MemoryStream();
        stream.Write(_data);
        return checked((int)stream.Length);
    }

    [Benchmark]
    public int SER_BUF_002_MemoryStream_Presize()
    {
        using var stream = new MemoryStream(_data.Length);
        stream.Write(_data);
        return checked((int)stream.Length);
    }

    [Benchmark]
    public int SER_BUF_003_ExactArray_Copy()
    {
        var output = new byte[_data.Length];
        _data.AsSpan().CopyTo(output);
        return output.Length;
    }

    [Benchmark]
    public int SER_BUF_004_ArrayPool_RentCopyReturn()
    {
        byte[] rented = System.Buffers.ArrayPool<byte>.Shared.Rent(_data.Length);
        try
        {
            _data.AsSpan().CopyTo(rented);
            return _data.Length;
        }
        finally
        {
            System.Buffers.ArrayPool<byte>.Shared.Return(rented);
        }
    }

    [Benchmark]
    public int SER_BUF_005_ArrayPool_RentCopyClearReturn()
    {
        byte[] rented = System.Buffers.ArrayPool<byte>.Shared.Rent(_data.Length);
        try
        {
            _data.AsSpan().CopyTo(rented);
            return _data.Length;
        }
        finally
        {
            System.Buffers.ArrayPool<byte>.Shared.Return(rented, clearArray: true);
        }
    }
}
