<!-- The Singularity Workshop moniker -->
<p align="center">
  <a href="https://github.com/TrentBest">
    <img src="https://raw.githubusercontent.com/TrentBest/TheSingularityWorkshop.Visualize/development/docs/assets/moniker.svg" alt="The Singularity Workshop — animated workshop moniker" width="720" />
  </a>
</p>

# ✳️ 00 FSM_Serialization_Benchmarks

This README follows the Workshop's shared [Documentation Standard](https://github.com/TrentBest/TheSingularityWorkshop.FSM_COS/blob/master/DOCUMENTATION_STANDARD.md), while keeping this repository's own purpose and implementation authoritative.

## Purpose

This repository is the evidence harness for `TheSingularityWorkshop.FSM_Serialization`. It preserves historical stream benchmarks and grows toward a reproducible matrix of primitive encoding, output-buffer strategies, mature serializer comparisons, and FSM-semantic workloads.

**Benchmark identity is permanent:** each operation has a stable `SER-...` ID embedded in its BenchmarkDotNet method name. See the [Benchmark Catalog](docs/BENCHMARK_CATALOG.md) before adding, renaming, retiring, or comparing a case.

## Current scope

- Historical BCL `MemoryStream` versus package stream/wrapper operations.
- Construction, read, write, `ToArray`, position overwrite, and a simple pack/unpack contract.
- New output-buffer strategy cases: growing/pre-sized `MemoryStream`, exact array copy, pooled buffer return, and pooled buffer clearing.
- Fixed-width UInt32 versus ULEB128 encode-only microbenchmarks, with setup-time round-trip validation over small, mixed and large-value distributions.
- Payload sizes 16 B, 1 KiB, 10 KB and 100 KB for the shared parameterized suites.

The current project references the published `TheSingularityWorkshop.FSM_Serialization` NuGet package at `0.1.0-alpha.2`. Keep this as the published-package lane until a deliberate version change is made. A source-build lane, if added, must be named and reported separately.

## Run

From the repository root, with the .NET 8 SDK installed:

```sh
dotnet restore
dotnet run -c Release -- --filter '*SER_*'
```

For the full suite, omit the filter. BenchmarkDotNet's generated artifacts should be retained with the source commit, package version, runtime, OS/CPU and catalog revision. Verify the filter against the installed BenchmarkDotNet version; if the command-line filter syntax differs, use the documented BenchmarkDotNet filter syntax for that version.

## Evidence discipline

- No performance winner is presumed; record results before making claims.
- Compare equivalent semantics and disclose when setup, copying, ownership or output allocation differs.
- Separate encode, decode and round-trip costs.
- Keep correctness checks independent from performance checks.
- Preserve raw result artifacts and historical baselines; do not overwrite old evidence.
- Do not publish packages from this benchmark repository.
- See [benchmarking methodology and future workload families](docs/BENCHMARK_CATALOG.md).

## Readiness

**Benchmark harness readiness: 40% (provisional).** Stable IDs/catalog, initial buffer-strategy cases, integer encoding comparisons, setup-time correctness validation, and a successful Windows/.NET 8 CI benchmark run are in place. FSM definition codecs, dedicated integer decode/round-trip timings, string/reference/layout technique comparisons, mature serializer adapters, automated result interpretation, and change-triggered observation remain outstanding. This is a harness implementation estimate, not a serialization package release-readiness percentage.
