using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Jobs;
using System.Buffers.Binary;
using System.IO;
using TheSingularityWorkshop.FSM_Serialization;

namespace FSM_Serialization_Benchmarks;

/// <summary>
/// Benchmarks the published FSM_Serialization package against the equivalent
/// BCL MemoryStream operations where a direct comparison is meaningful.
/// </summary>
[MemoryDiagnoser]
[SimpleJob(RuntimeMoniker.Net80)]
public class FSMSerializationBenchmarks
{
    [Params(16, 1024, 10_000, 100_000)]
    public int Size { get; set; }

    private byte[] _data = null!;
    private byte[] _readBuffer = null!;

    [GlobalSetup]
    public void Setup()
    {
        _data = new byte[Size];
        _readBuffer = new byte[Size];

        for (int i = 0; i < _data.Length; i++)
            _data[i] = (byte)(i * 31);
    }

    // ---------------------------------------------------------------------
    // Construction
    // ---------------------------------------------------------------------

    [Benchmark(Baseline = true)]
    public MemoryStream MemoryStream_Construct()
        => new();

    [Benchmark]
    public MemoryBinaryStream MemoryBinaryStream_Construct()
        => new();

    [Benchmark]
    public StreamBinaryStream StreamBinaryStream_Construct()
        => new(new MemoryStream());

    // ---------------------------------------------------------------------
    // Write
    // ---------------------------------------------------------------------

    [Benchmark(Baseline = true)]
    public MemoryStream MemoryStream_Write()
    {
        var stream = new MemoryStream();
        stream.Write(_data);
        return stream;
    }

    [Benchmark]
    public MemoryBinaryStream MemoryBinaryStream_Write()
    {
        var stream = new MemoryBinaryStream();
        stream.Write(_data);
        return stream;
    }

    [Benchmark]
    public StreamBinaryStream StreamBinaryStream_Write()
    {
        var stream = new StreamBinaryStream(new MemoryStream());
        stream.Write(_data);
        return stream;
    }

    // ---------------------------------------------------------------------
    // Read
    // ---------------------------------------------------------------------

    [Benchmark(Baseline = true)]
    public int MemoryStream_Read()
    {
        using var stream = new MemoryStream(_data, writable: false);
        return stream.Read(_readBuffer);
    }

    [Benchmark]
    public int MemoryBinaryStream_Read()
    {
        using var stream = new MemoryBinaryStream(_data);
        return stream.Read(_readBuffer);
    }

    [Benchmark]
    public int StreamBinaryStream_Read()
    {
        using var backing = new MemoryStream(_data, writable: false);
        using var stream = new StreamBinaryStream(backing);
        return stream.Read(_readBuffer);
    }

    // ---------------------------------------------------------------------
    // Existing-data construction
    // ---------------------------------------------------------------------

    [Benchmark(Baseline = true)]
    public MemoryStream MemoryStream_ConstructWithData()
        => new(_data, writable: false);

    [Benchmark]
    public MemoryBinaryStream MemoryBinaryStream_ConstructWithData()
        => new(_data);

    // ---------------------------------------------------------------------
    // ToArray
    // ---------------------------------------------------------------------

    [Benchmark(Baseline = true)]
    public byte[] MemoryStream_ToArray()
    {
        using var stream = new MemoryStream();
        stream.Write(_data);
        return stream.ToArray();
    }

    [Benchmark]
    public byte[] MemoryBinaryStream_ToArray()
    {
        using var stream = new MemoryBinaryStream();
        stream.Write(_data);
        return stream.ToArray();
    }

    // ---------------------------------------------------------------------
    // Position / overwrite
    // ---------------------------------------------------------------------

    [Benchmark(Baseline = true)]
    public void MemoryStream_PositionOverwrite()
    {
        using var stream = new MemoryStream(_data.ToArray());
        stream.Position = Size / 2;
        stream.WriteByte(0x7F);
    }

    [Benchmark]
    public void MemoryBinaryStream_PositionOverwrite()
    {
        using var stream = new MemoryBinaryStream(_data.ToArray());
        stream.Position = Size / 2;
        stream.Write([0x7F]);
    }

    // ---------------------------------------------------------------------
    // Pack / Unpack contract
    // ---------------------------------------------------------------------

    [Benchmark]
    public int BinarySerializable_RoundTrip()
    {
        var value = new BenchmarkValue(_data);

        using var stream = new MemoryBinaryStream();
        value.Pack(stream);

        stream.Position = 0;

        var restored = new BenchmarkValue(Size);
        restored.Unpack(stream);

        return restored.Payload.Length;
    }

    private sealed class BenchmarkValue : IBinarySerializable
    {
        public BenchmarkValue(byte[] payload)
        {
            Payload = payload;
        }

        public BenchmarkValue(int payloadLength)
        {
            Payload = new byte[payloadLength];
        }

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
