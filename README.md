<!-- The Singularity Workshop moniker -->
<p align="center">
  <a href="https://github.com/TrentBest">
    <img src="https://raw.githubusercontent.com/TrentBest/TheSingularityWorkshop.Visualize/development/docs/assets/moniker.svg" alt="The Singularity Workshop — animated workshop moniker" width="720" />
  </a>
</p>

# ✳️ 00 FSM_Serialization_Benchmarks

This repository is the reproducible performance-evidence harness for `TheSingularityWorkshop.FSM_Serialization`. It measures named operations, preserves raw results, and grows from primitive stream costs toward real format-owned FSM workloads.

This README follows the Workshop's shared [Documentation Standard](https://github.com/TrentBest/TheSingularityWorkshop.FSM_COS/blob/master/DOCUMENTATION_STANDARD.md), while keeping this repository's purpose and implementation authoritative.

## 🟦 01 What

BenchmarkDotNet cases have stable identities in the form `SER-<FAMILY>-<NNN>`. An ID names the operation and semantic contract, not a performance promise. See the [Benchmark Catalog](docs/BENCHMARK_CATALOG.md) before adding, retiring, or comparing a case.

Current measured families include:
- Historical stream construction, read/write, materialization, positioning, and pack/unpack baselines.
- Output-buffer strategies: growing/pre-sized `MemoryStream`, exact array copy, and pooled-buffer rent/copy/return with and without clearing.
- Fixed-width little-endian UInt32 versus ULEB128 encoding and decoding over small, mixed, and large-value distributions.

The catalog defines the exact contracts and caveats. These primitive cases do not establish the performance of an FSM definition codec that does not yet exist.

## 🟣 02 Why

Performance claims are useful only when another developer can reproduce the workload and understand what was timed. Stable IDs preserve history while raw reports, environment data, source identity, and correctness checks make results reviewable.

Do not compare operations with different semantics as if they were interchangeable. Do not attribute the approximately 21% end-to-end FSM_COS stress-test observation to serialization without a controlled, isolated experiment.

## 🔺 03 How

Requires the .NET 8 SDK. From this repository root:

```sh
dotnet restore
dotnet run --configuration Release -- --filter '*SER_*'
```

Omit the filter to run the full suite. Verify filter syntax against the BenchmarkDotNet version restored by this repository.

The automated workflow separates two lanes:

- **Published-package baseline:** pinned to `TheSingularityWorkshop.FSM_Serialization` NuGet `0.1.0-alpha.2` for historical consumer comparisons.
- **Development-source lane:** tests the source repository first, then builds and benchmarks a direct project reference at the requested full source SHA (or the configured development ref).

Never merge or compare the lanes as if they were the same build. Each run should preserve the source/harness commits, package version or source identity, runtime, OS/CPU, configuration, parameters, BenchmarkDotNet reports, and correctness result.

## 🟢 04 See it in a minute

1. Read the [Benchmark Catalog](docs/BENCHMARK_CATALOG.md) and select a stable ID.
2. Run the Release command above.
3. Inspect the generated `BenchmarkDotNet.Artifacts/results` reports.
4. Record the exact source commit and environment with the result.
5. Compare only equivalent workloads and keep the raw artifacts.

A successful workflow proves that the configured cases ran and reports were retained. It does not, by itself, prove that one implementation is faster, that a regression threshold is met, or that the complete serialization domain has been benchmarked.

## 🟩 05 Readiness and next work

**Harness readiness: 45% (provisional; not a package release verdict).** The stable catalog, initial buffer-strategy cases, integer encode/decode cases, setup-time correctness checks, and distinct published/source lanes are implemented. The dual-lane workflow run [#18](https://github.com/TrentBest/FSM_Serialization_Benchmarks/actions/runs/40252238764) passed both lanes and retained BenchmarkDotNet artifacts. That validates execution and report production—not comparative analysis or domain completeness.

Outstanding work includes:
- Full integer round-trip and signed/zigzag cases.
- String, reference/table, layout/sizing, and concrete FSM definition workloads once their format contract is owned and implemented.
- Fair adapters for mature serializer comparisons, with equivalent semantics and explicit setup/allocation costs.
- Malformed-input/resource-safety workloads where meaningful.
- Automated result interpretation using repeatable comparisons, noise-aware regression policy, and historical deltas.
- Repair and verify cross-repository dispatch: the source observer has encountered a `403 Resource not accessible by personal access token`; a configured secret alone does not prove repository-dispatch permission.
- A complete local link/asset check and a valid `Avatar.jpg` footer asset. The required `Avatar.jpg` asset was not found in this repository's checked development paths, so this README does not fabricate a link to it.

## Evidence discipline

- Correctness and performance are separate gates.
- Run source tests before source benchmarks.
- Preserve raw reports; never overwrite historical evidence.
- Keep setup, copying, allocations, buffer ownership, and cleanup costs explicit.
- Treat a performance regression as actionable only after confirming semantic equivalence and sufficient repeatability; do not fail on a single noisy sample.
- Do not publish packages from this repository.
- FSM_Serialization package publication remains a separate, explicitly authorized release action.

## Support and related documentation

- [Benchmark catalog and methodology](docs/BENCHMARK_CATALOG.md)
- [FSM_Serialization source repository](https://github.com/TrentBest/TheSingularityWorkshop.FSM_Serialization)
- [FSM_Serialization domain completeness review](https://github.com/TrentBest/TheSingularityWorkshop.FSM_Serialization/blob/development/docs/DOMAIN_COMPLETENESS.md)
- [Shared Workshop documentation standard](https://github.com/TrentBest/TheSingularityWorkshop.FSM_COS/blob/master/DOCUMENTATION_STANDARD.md)
- [Workshop GitHub](https://github.com/TrentBest)
- [Support the Workshop](https://www.patreon.com/TheSingularityWorkshop)

---

<p align="center">
  <a href="https://github.com/TrentBest/TheSingularityWorkshop" aria-label="The Singularity Workshop living avatar">
    <img src="https://raw.githubusercontent.com/TrentBest/WebPage/development/TheSingularityWorkshop/wwwroot/Images/TheSingularityWorkshopLivingAvatar.svg" alt="The Singularity Workshop living avatar — a digital vortex converging on its central eye" width="112" />
  </a><br />
  <strong>The Singularity Workshop</strong><br />
  <em>If a picture is worth a thousand words, what is a living picture worth?</em><br />
  <a href="https://github.com/TrentBest">GitHub</a> · <a href="https://www.patreon.com/TheSingularityWorkshop">Support the Workshop</a>
</p>
