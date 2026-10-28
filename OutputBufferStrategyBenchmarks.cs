using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Jobs;
using System;
using System.IO;
using System.Buffers;

namespace FSM_Serialization_Benchmarks;

/// <summary>Isolates output-buffer strategies over the same deterministic byte payload.</summary>
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
        byte[] rented = ArrayPool<byte>.Shared.Rent(_data.Length);
        try
        {
            _data.AsSpan().CopyTo(rented);
            return _data.Length;
        }
        finally
        {
            ArrayPool<byte>.Shared.Return(rented);
        }
    }

    [Benchmark]
    public int SER_BUF_005_ArrayPool_RentCopyClearReturn()
    {
        byte[] rented = ArrayPool<byte>.Shared.Rent(_data.Length);
        try
        {
            _data.AsSpan().CopyTo(rented);
            return _data.Length;
        }
        finally
        {
            ArrayPool<byte>.Shared.Return(rented, clearArray: true);
        }
    }
}
