<!-- The Singularity Workshop moniker -->
<p align="center"><a href="https://github.com/TrentBest"><img src="https://raw.githubusercontent.com/TrentBest/TheSingularityWorkshop.Visualize/development/docs/assets/moniker.svg" alt="The Singularity Workshop animated moniker" width="720" /></a></p>

# FSM_Serialization_Benchmarks — Documentation Coverage Inventory

This inventory applies the [FSM_COS Documentation Standard](https://github.com/TrentBest/TheSingularityWorkshop.FSM_COS/blob/master/DOCUMENTATION_STANDARD.md) to this benchmark harness. It records what the repository documents today and what remains planned; a listed future artifact is not evidence that a generator, test, or workflow exists.

## Coverage at a glance

| View / artifact | Status | Current evidence and next step |
|---|---|---|
| Reader journey / README | **Implemented, source-checked** | [README](../README.md) explains purpose, why, how, first run, limits, and next steps. The current branch still lacks an evidenced build/run. |
| Living Gallery | **Implemented, authored conceptual** | [Gallery](GALLERY.md) links the visual model to the catalog and reproducibility steps. The animation is not telemetry. |
| Benchmark identity and timed contracts | **Implemented, source-checked** | [Benchmark Catalog](BENCHMARK_CATALOG.md) lists stable IDs, measured operations, parameters, and caveats. |
| Architecture / responsibility boundary | **Manual, partial** | README distinguishes harness, serializer package, correctness gate, measurement, and evidence. A fuller component view may be added when the execution pipeline is automated. |
| Data flow / evidence lineage | **Implemented, authored conceptual** | [serialization-evidence-flow.svg](assets/serialization-evidence-flow.svg) shows fixture → operation → correctness → timing → retained reports. It does not derive its facts from runtime telemetry. |
| Activity / control flow | **Planned** | Add a source-checked view of benchmark setup, validation, measured invocation, and report production after the executable pipeline is established. |
| Sequence diagram | **Planned** | Capture the real orchestration path (CI/CiAi → source tests → benchmark run → artifact retention) only after its actual execution boundary is confirmed. |
| Type diagrams / per-type pages | **Planned / scope to inventory** | Inventory declared types and decide which are meaningful public or harness concepts. Do not claim repository-wide source-derived UML yet. |
| Use cases | **Manual, partial** | Benchmark scenarios and intent are recorded in the catalog. A separate actor/goal/precondition/outcome/failure set is not yet complete. |
| State / lifecycle diagrams | **Not applicable to the current narrow view; reassess** | No deployed long-running service is documented here. Reassess if a durable result-store or orchestration lifecycle is added. |
| Deployment / runtime topology | **Not applicable to current implementation; reassess** | The current harness is a local .NET benchmark executable. CI topology becomes applicable when the automated execution lane is restored. |
| MicroBundle / manifest views | **Not applicable** | This repository neither defines nor composes MicroBundles. |
| Requirements ↔ tests ↔ evidence traceability | **Planned** | Link each benchmark contract and correctness check to tests, exact source identity, raw report, and comparison history. |
| Link, asset, accessibility, stale-output gate | **Not yet automated** | Keep alt text and SVG title/description aligned; add automated validation when the CiAi/documentation gate supports it. |
| Canonical footer asset | **Blocked by missing source asset** | `Avatar.jpg` was not found in the checked development tree or searched Workshop source paths. Do not invent a URL or silently substitute another image. |

## Evidence labels

- **Verified:** built/run against a named source revision and environment.
- **Source-checked:** checked against current source or documents, but not executed.
- **Authored conceptual:** human-authored explanation that communicates intent or a model; not proof of implementation behavior.
- **Planned:** desired artifact or automation that does not yet exist as verified evidence.
- **Not applicable:** omitted with a reason and subject to reassessment when the repository changes.

## Living-picture contract

Each new diagram should state:

1. The question it helps a reader answer.
2. Whether it is source-derived, authored conceptual, or captured runtime evidence.
3. Its source revision, analysis scope, and unresolved relationships where applicable.
4. What it proves and what it does not prove.
5. How to update or regenerate it.
6. An accessible title, description, and equivalent explanatory text.
7. A link to the deeper contract, recipe, or test that owns the details.

**Animation is not automatically interactivity, and interactivity is not automatically live data.** Label each accurately. Prefer a useful static reading when motion is unavailable, and never imply a live result from an authored illustration.

## Completion gate

Documentation is not complete because every row has a file. Close the gate only when applicable views have an honest status, the links and assets are checked, source-derived outputs identify their provenance and scope, and any claimed automated coverage is demonstrated by the actual workflow.

<!-- Canonical Avatar.jpg footer not yet applied: no verified Avatar.jpg asset path was found. Do not substitute another image or invent a URL. -->
