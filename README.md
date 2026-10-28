<!-- The Singularity Workshop moniker -->
<p align="center"><a href="https://github.com/TrentBest"><img src="https://raw.githubusercontent.com/TrentBest/TheSingularityWorkshop.Visualize/development/docs/assets/moniker.svg" alt="The Singularity Workshop animated moniker" width="720" /></a></p>

# ✳️ 00 FSM_Serialization_Benchmarks

[![Last commit](https://img.shields.io/github/last-commit/TrentBest/FSM_Serialization_Benchmarks/development?style=flat-square&logo=github)](https://github.com/TrentBest/FSM_Serialization_Benchmarks/commits/development/)
[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](LICENSE.txt)

This repository is the reproducible performance-evidence harness for `TheSingularityWorkshop.FSM_Serialization`. It measures named operations, preserves raw results, and grows from primitive stream costs toward real format-owned FSM workloads.

This README follows the Workshop's shared [Documentation Standard](https://github.com/TrentBest/TheSingularityWorkshop.FSM_COS/blob/master/DOCUMENTATION_STANDARD.md), while keeping this repository's purpose and implementation authoritative.

<p align="center">
  <a href="docs/GALLERY.md"><img src="docs/assets/serialization-evidence-flow.svg" alt="Conceptual living diagram: deterministic input passes through a named operation and correctness gate before timing and evidence retention. A failed correctness gate invalidates performance interpretation." width="1000" /></a>
</p>

**[Explore the living Gallery](docs/GALLERY.md)** · **[Read the Benchmark Catalog](docs/BENCHMARK_CATALOG.md)** · **[Review documentation coverage](docs/DOCUMENTATION_INVENTORY.md)**

## 🟦 01 What

BenchmarkDotNet cases have stable identities in the form `SER-<FAMILY>-<NNN>`. An ID names the operation and semantic contract, not a performance promise. See the [Benchmark Catalog](docs/BENCHMARK_CATALOG.md) before adding, retiring, or comparing a case.

Current measured families include:
- Stream construction, read/write, materialization, positioning, and pack/unpack baselines are present on `development`.
- Output-buffer strategies (`SER-BUF-001..005`) and primitive UInt32 encoding/decoding (`SER-INT-001..004`) are now present on `development`. Their inclusion is not evidence of a successful benchmark run until CI builds the harness and retains reports for these cases.
- Current workload parameters are 16, 1,024, 10,000, and 100,000 bytes.

The catalog defines the exact contracts and caveats. These primitive cases do not establish the performance of an FSM definition codec that does not yet exist.

## 🟣 02 Why

Performance claims are useful only when another developer can reproduce the workload and understand what was timed. Stable IDs preserve history while raw reports, environment data, source identity, and correctness checks make results reviewable.

Do not compare operations with different semantics as if they were interchangeable. Do not attribute the approximately 21% end-to-end FSM_COS stress-test observation to serialization without a controlled, isolated experiment.

## 🔺 03 How

Requires the .NET 8 SDK. From this repository root:

```sh
dotnet restore
dotnet run --configuration Release
```

The intended automated design separates two lanes:
- **Published-package baseline:** pinned to `TheSingularityWorkshop.FSM_Serialization` NuGet `0.1.0-alpha.2` for historical consumer comparisons.
- **Development-source lane:** tests the source repository first, then builds and benchmarks a direct project reference at the requested full source SHA (or configured development ref).

**Verified CI status (2026-10-28):** `.github/workflows/benchmark.yml` on `development` now runs the exact-source smoke and a controlled comparison against pinned NuGet `0.1.0-alpha.2`. [Run #40781193957](https://github.com/TrentBest/FSM_Serialization_Benchmarks/actions/runs/40781193957) passed source build/tests, the 44-case Dry correctness gate, five independent baseline launches, five exact-source candidate launches, CiAi policy evaluation, and evidence upload. For the recorded `SER-RT-001 | BinarySerializable_RoundTrip` configuration `Size=16`, CiAi compared medians of 86.09 ns (published package) and 85.98 ns (exact source), a -0.1278% change, and returned `passed`. Artifact [serialization-benchmark-evidence-40781193957-1](https://github.com/TrentBest/FSM_Serialization_Benchmarks/actions/runs/40781193957/artifacts/12857069013) is retained for 90 days. **Scope caveat:** the controlled CSV parser currently selects only the first report row (`Size=16`), not all four parameter sizes. This result is valid for that configuration only; it is not a verdict for sizes 1,024, 10,000, or 100,000, and the regression-failure path still needs explicit host verification. The older cross-repository dispatch run [#40637381835](https://github.com/TrentBest/FSM_Serialization_Benchmarks/actions/runs/40637381835) remains evidence that the existing `master` workflow can run both the development-source and pinned published-package lanes; it is not the same as the newer development host adapter.

Never merge or compare the lanes as if they were the same build. Each run should preserve the source/harness commits, package version or source identity, runtime, OS/CPU, configuration, parameters, BenchmarkDotNet reports, and correctness result.

## 🟢 04 See it in a minute

1. Read the [Benchmark Catalog](docs/BENCHMARK_CATALOG.md) and select a stable ID.
2. Read the [Living Gallery](docs/GALLERY.md) to understand the correctness-to-evidence flow.
3. Run the Release command above.
4. Inspect the generated `BenchmarkDotNet.Artifacts/results` reports.
5. Record the exact source commit and environment with the result.

A successful workflow proves that the configured cases ran and reports were retained. It does not, by itself, prove that one implementation is faster, that a regression threshold is met, or that the complete serialization domain has been benchmarked.

## 🟪 05 Readiness and next work

**Harness readiness: provisional; not a package release verdict.** The first repeated-measurement comparison and CiAi `passed` outcome are verified in [run #40781193957](https://github.com/TrentBest/FSM_Serialization_Benchmarks/actions/runs/40781193957), but only for `SER-RT-001` at `Size=16`. The living evidence-flow diagram remains conceptual documentation, not a live dashboard. Broader per-parameter comparison and explicit regression-path verification remain outstanding.

Outstanding work includes:
- Extend controlled comparison parsing so each intended `Size` parameter (16, 1,024, 10,000, 100,000) gets its own comparable baseline/candidate evidence and CiAi verdict; do not pool unlike parameter sizes into one median. Keep at least five valid samples per side and preserve the package version explicitly in provenance.
- Run the new setup-time byte-for-byte round-trip validation under BenchmarkDotNet; the code change is committed but still needs an actual current-branch build/run.
- Run the reconciled output-buffer (`SER-BUF-001..005`) and integer (`SER-INT-001..004`) families on `development`, retain reports, and compare only equivalent operations under recorded conditions.
- Extend integer coverage with signed zigzag and complete encode/decode round-trip cases after the current families have been measured and reviewed.
- Add string, reference/table, layout/sizing, and concrete FSM definition workloads after the relevant format contract exists.
- Add fair adapters for mature serializer comparisons with equivalent semantics and explicit setup/allocation costs.
- Add malformed-input/resource-safety workloads where meaningful.
- Exercise CiAi's actual `regression` outcome in the host workflow using controlled fixture/evidence injection or another deterministic test path, and preserve policy results alongside raw reports for historical deltas. The real `passed` path is verified for `Size=16` in run #40781193957.
- Verify cross-repository dispatch and the actual workflow files/runs before claiming the pipeline is healthy. The existing successful full dispatch used the `master` workflow; the current `development` workflow still needs its own full exact-source run.
- Resolve the documentation footer asset gap: `Avatar.jpg` was not found in the checked development tree or the searched Workshop source paths. Do not invent a path or imply the living SVG is the canonical footer replacement.

## Evidence discipline

- Correctness and performance are separate gates.
- Run source tests before source benchmarks.
- Preserve raw reports; never overwrite historical evidence.
- Keep setup, copying, allocations, buffer ownership, and cleanup costs explicit.
- Treat a performance regression as actionable only after confirming semantic equivalence and sufficient repeatability; do not fail on a single noisy sample.
- Do not publish packages from this repository.
- FSM_Serialization package publication remains a separate, explicitly authorized release action.

## Support and related documentation

- [Living Gallery](docs/GALLERY.md)
- [Benchmark catalog and methodology](docs/BENCHMARK_CATALOG.md)
- [Documentation coverage inventory](docs/DOCUMENTATION_INVENTORY.md)
- [FSM_Serialization source repository](https://github.com/TrentBest/TheSingularityWorkshop.FSM_Serialization)
- [FSM_Serialization domain completeness review](https://github.com/TrentBest/TheSingularityWorkshop.FSM_Serialization/blob/development/docs/DOMAIN_COMPLETENESS.md)
- [Shared Workshop documentation standard](https://github.com/TrentBest/TheSingularityWorkshop.FSM_COS/blob/master/DOCUMENTATION_STANDARD.md)
- [Workshop GitHub](https://github.com/TrentBest)
- [Support the Workshop](https://www.patreon.com/TheSingularityWorkshop)

---

<!-- Canonical Avatar.jpg footer not yet applied: no verified Avatar.jpg asset path was found. Do not substitute another image or invent a URL. -->
