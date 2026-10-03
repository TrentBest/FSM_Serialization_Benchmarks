# FSM_Serialization_Benchmarks

This repository contains the executable performance experiments for **TheSingularityWorkshop.FSM_Serialization**.

The benchmark project is separate from the production package because a benchmark is an experiment: it should show exactly what was measured, how it was measured, and how the result should be interpreted.

## Why benchmark serialization?

Serialization performance is easy to misunderstand. A small abstraction cost can disappear inside a large payload operation, while a convenient constructor can hide a large memory copy.

This suite compares the Workshop stream implementations with the .NET `MemoryStream` baseline for:

- construction;
- writing;
- reading;
- construction from existing data;
- `ToArray`;
- position/overwrite;
- an end-to-end `IBinarySerializable` round trip.

Payload sizes are **16, 1,024, 10,000, and 100,000 bytes**.

## Package and benchmark versions

These recorded results were produced against:

- `TheSingularityWorkshop.FSM_Serialization 0.1.0-alpha.2`
- BenchmarkDotNet `0.15.2`
- .NET `8.0.31`
- Windows 10 22H2
- Intel Core i5-10400F, 6 physical / 12 logical cores
- RyuJIT AVX2

These are historical measurements of that package version and environment. They are not performance guarantees for later releases or other machines.

## Recorded results

The most useful large-payload measurements from the first benchmark pass are:

| Operation | Size | MemoryStream | FSM_Serialization | Relative result |
|---|---:|---:|---:|---:|
| Construct empty | 16–100,000 | ~7.1–7.3 ns | ~10.2–10.7 ns | ~1.4–1.5× |
| Write | 100,000 B | 39,894.85 ns | 40,005.59 ns | ~1.00× |
| Read via StreamBinaryStream | 100,000 B | 2,272.47 ns | 2,298.40 ns | ~1.01× |
| ToArray | 100,000 B | 80,063.31 ns | 80,227.68 ns | ~1.00× |
| Position + overwrite | 100,000 B | 39,824.53 ns | 80,220.26 ns | ~2.01× |
| Pack + Unpack | 100,000 B | — | 60,152.74 ns | end-to-end contract |

### What the results establish

At 100 KB, bulk write is effectively at the same measured speed as the `MemoryStream` baseline:

~~~text
MemoryStream          39,894.85 ns
MemoryBinaryStream    40,005.59 ns
~~~

Reading through `StreamBinaryStream` is similarly close:

~~~text
MemoryStream read          2,272.47 ns
StreamBinaryStream read    2,298.40 ns
~~~

And `ToArray` converges at the larger payload:

~~~text
MemoryStream.ToArray()        80,063.31 ns
MemoryBinaryStream.ToArray()  80,227.68 ns
~~~

The useful conclusion is not that every operation is identical. It is that **for large sequential operations, the representation boundary can add very little measured overhead compared with the work of moving the payload**.

## The important caveat: copying is not abstraction overhead

The current `MemoryBinaryStream(byte[])` constructor copies the supplied data.

The .NET comparison:

~~~csharp
new MemoryStream(Data, writable: false)
~~~

can wrap the existing array without copying it.

Therefore this comparison:

~~~text
MemoryStream(byte[])
MemoryBinaryStream(byte[])
~~~

is partly a measurement of **copying strategy**, not merely interface or wrapper overhead.

At 100 KB, construction measured approximately:

| Implementation | Mean | Allocated |
|---|---:|---:|
| MemoryStream(byte[]) | 6.05 ns | 64 B |
| MemoryBinaryStream(byte[]) | 39,975.93 ns | 100,122 B |

The corresponding read benchmark exposed the same design cost:

| Implementation | Mean | Allocated |
|---|---:|---:|
| MemoryStream | 2,272.47 ns | 64 B |
| MemoryBinaryStream | 43,128.59 ns | 100,122 B |

Those numbers should **not** be described as "the abstraction is 19× slower." The experiment primarily exposes the current copy.

That distinction is one of the reasons this benchmark repository exists.

## End-to-end round trip

The suite also measures:

~~~text
BenchmarkValue
     │
    Pack
     ▼
MemoryBinaryStream
     │
   rewind
     ▼
BenchmarkValue
     │
   Unpack
     ▼
restored payload
~~~

Recorded means:

| Payload | Pack + Unpack |
|---:|---:|
| 16 B | 64.08 ns |
| 1,024 B | 207.47 ns |
| 10,000 B | 1,481.61 ns |
| 100,000 B | 60,152.74 ns |

This is deliberately an end-to-end measurement. It includes test-value creation, stream creation, packing, rewinding, destination creation, payload allocation, and unpacking.

It is therefore a useful regression baseline, not a claim about the cost of `Pack` alone.

## How to find the benchmark

The source is:

~~~text
FSM_Serialization Benchmarks.cs
~~~

Look for:

- `[MemoryDiagnoser]` — allocation measurements are enabled.
- `[SimpleJob(RuntimeMoniker.Net80)]` — the runtime is explicit.
- `[Params(16, 1024, 10_000, 100_000)]` — payload size is the experimental variable.
- `[Benchmark(Baseline = true)]` — the local BCL comparison.
- `[Benchmark]` — the Workshop implementation being measured.

This gives the reader a direct trail:

~~~text
package claim
    │
    ▼
benchmark repository
    │
    ▼
benchmark class
    │
    ▼
[Benchmark] method
    │
    ▼
BenchmarkDotNet result
    │
    ▼
interpretation
~~~

## How to read BenchmarkDotNet output

### Mean

The average measured duration for one benchmark operation.

For example, `40,005.59 ns` is approximately 40 microseconds.

### Allocated

Managed memory allocated by one benchmark operation.

Time and allocation answer different questions:

~~~text
CPU cost
   +
managed allocation
   =
two different performance dimensions
~~~

### Baseline ratio

A ratio compares the measured operation with the benchmark selected as its baseline. It is a local experimental comparison, not a universal performance score.

## Reproducing the experiment

Run in Release configuration:

~~~bash
dotnet run -c Release
~~~

For meaningful comparisons, keep the benchmark source, package version, runtime, payload sizes, and BenchmarkDotNet version stable.

Benchmark output is environment-sensitive. Processor, JIT, OS, thermal state, background activity, and runtime version can all move the numbers.

## Why there is no ordinary CI performance gate

These experiments are different from correctness tests.

A unit test asks:

> Did the operation behave correctly?

A benchmark asks:

> How expensive was the operation on this machine under this workload?

Because performance numbers move with the environment, this repository is an **executable experiment archive** rather than a claim that every commit has an immutable performance score.

A meaningful performance change should trigger a fresh benchmark run.

## Relationship to the package

Production package:

- [TheSingularityWorkshop.FSM_Serialization](https://github.com/TrentBest/TheSingularityWorkshop.FSM_Serialization)

Benchmark experiment:

- [FSM_Serialization_Benchmarks](https://github.com/TrentBest/FSM_Serialization_Benchmarks)

The package README summarizes the important evidence; this repository holds the experiment itself.

## When should we run it again?

Rerun this suite after changes to:

- `MemoryBinaryStream`;
- `StreamBinaryStream`;
- buffer ownership;
- copying behavior;
- `IBinaryStream`;
- packing/unpacking infrastructure;
- allocation strategy;
- representation primitives;
- runtime target.

**Measure first. Change one thing. Measure again.**
