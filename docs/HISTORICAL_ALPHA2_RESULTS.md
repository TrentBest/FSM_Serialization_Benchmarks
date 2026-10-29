<!-- The Singularity Workshop moniker -->
<p align="center"><a href="https://github.com/TrentBest"><img src="https://raw.githubusercontent.com/TrentBest/TheSingularityWorkshop.Visualize/development/docs/assets/moniker.svg" alt="The Singularity Workshop animated moniker" width="720" /></a></p>

# FSM_Serialization_Benchmarks — Historical alpha.2 Results

This page preserves the first published-package benchmark observations so later measurements do not silently erase the original evidence. These are **historical measurements**, not current performance claims or guarantees.

See the [current README](../README.md) for the active correctness-gated/CiAi workflow and the [Benchmark Catalog](BENCHMARK_CATALOG.md) for current workload contracts and comparability caveats.

## Provenance

Recorded against TheSingularityWorkshop.FSM_Serialization 0.1.0-alpha.2, BenchmarkDotNet 0.15.2, .NET 8.0.31, Windows 10 22H2, Intel Core i5-10400F (6 physical / 12 logical cores), RyuJIT AVX2.

The legacy summary does not include all raw per-iteration observations. Treat the values as reported means from the original benchmark pass; do not combine them statistically with newer CI medians.

## Reported stream-operation measurements

| Operation | Payload | MemoryStream | FSM_Serialization | Interpretation |
|---|---:|---:|---:|---|
| Construct empty | 16–100,000 B | ~7.1–7.3 ns | ~10.2–10.7 ns | Construction only |
| Write | 100,000 B | 39,894.85 ns | 40,005.59 ns | Approximately equal measured bulk-write cost |
| Read via stream | 100,000 B | 2,272.47 ns | 2,298.40 ns | Approximately equal for this reported operation |
| ToArray | 100,000 B | 80,063.31 ns | 80,227.68 ns | Approximately equal materialization cost |
| Position + overwrite | 100,000 B | 39,824.53 ns | 80,220.26 ns | Candidate case was about 2.01× the reported baseline |
| Pack + Unpack | 100,000 B | — | 60,152.74 ns | End-to-end fixture operation; no BCL equivalent shown |

## The copy-versus-wrap caveat

The original constructor comparison is not an isolated abstraction comparison. The expression new MemoryStream(data, writable: false) can wrap an existing array, while MemoryBinaryStream(byte[]) copied the supplied data into an expandable internal stream in the measured implementation.

| Constructor at 100 KB | Reported mean | Reported allocation |
|---|---:|---:|
| MemoryStream over existing array | 6.05 ns | 64 B |
| MemoryBinaryStream from supplied array | 39,975.93 ns | 100,122 B |

The corresponding read comparison reported:

| Read case at 100 KB | Reported mean | Reported allocation |
|---|---:|---:|
| MemoryStream over existing array | 2,272.47 ns | 64 B |
| MemoryBinaryStream from supplied array | 43,128.59 ns | 100,122 B |

These figures primarily expose a copy-versus-wrap difference and its allocation cost. They must **not** be summarized as “the abstraction is 19× slower.” The operations differ in memory ownership and setup work.

## Reported end-to-end Pack + Unpack means

| Payload | Mean |
|---:|---:|
| 16 B | 64.08 ns |
| 1,024 B | 207.47 ns |
| 10,000 B | 1,481.61 ns |
| 100,000 B | 60,152.74 ns |

This end-to-end fixture included value creation, stream creation, packing, rewinding, destination creation, payload allocation, and unpacking. It is not an isolated measurement of Pack or Unpack.

## How to use this record

- Keep this evidence attached to its original package version and environment.
- Do not compare these means directly to newer medians or different machine/runtime results.
- Read the [current catalog](BENCHMARK_CATALOG.md) before comparing cases; setup, ownership, allocation, and semantics must match.
- Use fresh controlled measurements for present-day decisions. The current pipeline records independent launches and exact source/package provenance.

These observations are useful as a historical baseline and as a reminder that copy strategy can dominate apparent stream performance. They do not establish the performance of a production FSM definition codec or of later package versions.

---

<!-- Canonical Avatar.jpg footer not yet applied: no verified Avatar.jpg asset path was found. Do not substitute another image or invent a URL. -->
