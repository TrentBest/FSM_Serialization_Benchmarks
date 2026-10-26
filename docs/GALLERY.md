<!-- The Singularity Workshop moniker -->
<p align="center"><a href="https://github.com/TrentBest"><img src="https://raw.githubusercontent.com/TrentBest/TheSingularityWorkshop.Visualize/development/docs/assets/moniker.svg" alt="The Singularity Workshop animated moniker" width="720" /></a></p>

# FSM_Serialization_Benchmarks — Living Gallery

## 01 — What does this gallery show?

**A benchmark is a claim with a contract.** The workload says what is measured; correctness says whether the result is meaningful; retained evidence lets another person inspect what actually ran.

The visuals here are explanatory models, not live benchmark telemetry. Every exhibit links to the document that owns its detailed contract. The animated pulse illustrates a conceptual flow only—it does not consume current results or prove that any workflow has passed.

## 02 — Why make the evidence visible?

A single number can hide setup costs, allocation, copying, disposal, and semantic differences. The point of a living picture is to make those boundaries easier to notice before a reader compares two results.

## 03 — How to read the evidence flow

![Living conceptual diagram: a fixture passes through a named operation and correctness gate before timing and evidence retention. A failed correctness gate invalidates performance interpretation.](assets/serialization-evidence-flow.svg)

Read from left to right:

1. **Fixture:** deterministic inputs and declared parameters establish the workload.
2. **Operation:** a stable benchmark ID names the exact contract being measured.
3. **Correctness gate:** restored data must match the fixture before speed is interpreted.
4. **Measurement:** setup, copying, allocation, buffer ownership, and cleanup costs must be explicit.
5. **Evidence:** retain raw reports with the harness/source identity and machine/runtime details.

The lower branches matter just as much as the main line. A correctness failure blocks performance interpretation. A benchmark report is not automatically a win, regression, or proof of domain completeness.

## 04 — See it in a minute

- **Choose a workload:** [Benchmark Catalog](BENCHMARK_CATALOG.md) defines stable IDs, timed contracts, parameters, and caveats.
- **Run the harness:** from the repository root, use `dotnet restore` followed by `dotnet run --configuration Release`.
- **Inspect the report:** look under `BenchmarkDotNet.Artifacts/results`.
- **Verify the evidence:** record the exact harness commit, package version or source SHA, runtime, OS/CPU, configuration, parameters, and correctness status.

**Current evidence limitation:** the repository's `development` branch has not yet demonstrated a current-branch build and benchmark run for the latest correctness changes. The diagram explains the intended evidence discipline; it is not a claim that the pipeline is currently green.

## 05 — Choose your next step

- **Understand each measured operation:** [Benchmark Catalog](BENCHMARK_CATALOG.md).
- **Understand purpose, setup, and current limitations:** [Repository README](../README.md).
- **Understand serialization domain gaps:** [FSM_Serialization domain completeness review](https://github.com/TrentBest/TheSingularityWorkshop.FSM_Serialization/blob/development/docs/DOMAIN_COMPLETENESS.md).
- **Understand the shared standard:** [FSM_COS Documentation Standard](https://github.com/TrentBest/TheSingularityWorkshop.FSM_COS/blob/master/DOCUMENTATION_STANDARD.md).

---

## Visual provenance

- **Artifact:** `serialization-evidence-flow.svg`
- **Status:** authored conceptual model; animated explanatory pulse; not runtime telemetry.
- **Source revision:** the commit adding this artifact and this gallery.
- **Regeneration:** edit the SVG source directly; keep its title, accessible description, and matching Markdown text alternative synchronized.
- **Verification limit:** GitHub rendering of the SVG is the presentation target; no interactive controls or live-data binding are claimed.

<!-- THE SINGULARITY WORKSHOP FOOTER -->
<p align="center"><img src="https://raw.githubusercontent.com/TrentBest/FSM_API/master/Documentation/Branding/TheSingularityWorkshop.png" alt="The Singularity Workshop avatar" width="200" /></p>
