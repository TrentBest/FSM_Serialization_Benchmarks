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
- Stream construction, read/write, materialization, positioning, and pack/unpack baselines.
- Output-buffer strategy and primitive integer-encoding cases are planned follow-up families; do not infer that they are present in the current harness source until the corresponding cases appear in the catalog.
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

**Current CI caveat:** the current `development` tree contains no `.github/workflows/` files, and the Actions API reports no workflow runs on this branch. Therefore the two-lane automation described above is not currently evidenced as active on this branch. Historical run [#18](https://github.com/TrentBest/FSM_Serialization_Benchmarks/actions/runs/40252238764) must not be treated as proof that this exact tree was built.

Never merge or compare the lanes as if they were the same build. Each run should preserve the source/harness commits, package version or source identity, runtime, OS/CPU, configuration, parameters, BenchmarkDotNet reports, and correctness result.

## 🟢 04 See it in a minute

1. Read the [Benchmark Catalog](docs/BENCHMARK_CATALOG.md) and select a stable ID.
2. Run the Release command above.
3. Inspect the generated `BenchmarkDotNet.Artifacts/results` reports.
4. Record the exact source commit and environment with the result.
5. Compare only equivalent workloads and keep the raw artifacts.

A successful workflow proves that the configured cases ran and reports were retained. It does not, by itself, prove that one implementation is faster, that a regression threshold is met, or that the complete serialization domain has been benchmarked.

## 🟩 05 Readiness and next work

**Harness readiness: provisional; not a package release verdict.** Stable names are now attached to the current stream/round-trip cases, and a catalog records their timed contracts and caveats. No current-branch CI pass is claimed.

Outstanding work includes:
- Restore automated validation for this branch, including restore/build and benchmark execution with retained reports.
- Run the new setup-time byte-for-byte round-trip validation under BenchmarkDotNet; the code change is committed but still needs an actual current-branch build/run.
- Add integer fixed-width/variable-length encoding, signed zigzag, and complete round-trip cases.
- Add string, reference/table, layout/sizing, and concrete FSM definition workloads after the relevant format contract exists.
- Add fair adapters for mature serializer comparisons with equivalent semantics and explicit setup/allocation costs.
- Add malformed-input/resource-safety workloads where meaningful.
- Implement automated result interpretation with repeated comparisons, noise-aware regression policy, and historical deltas.
- Verify cross-repository dispatch and the actual workflow files/runs before claiming the pipeline is healthy.
- Run a complete link/asset check and provide a valid `Avatar.jpg` footer asset. That asset was not found in the checked development tree, so this README does not fabricate a link to it.

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
