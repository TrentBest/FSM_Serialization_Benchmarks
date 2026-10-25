# Serialization Benchmark Catalog

**Catalog schema:** `SER-<FAMILY>-<NNN>`. IDs are permanent identifiers; display names, implementation names, and source paths may change. Never reuse an ID for a different operation or semantic contract. Retired benchmarks remain in this catalog with their retirement reason. Add new IDs; do not renumber history.

## Benchmark identity rules

Each recorded run should retain:

- Stable benchmark ID and catalog revision.
- Benchmark implementation name and source commit.
- Target package name/version and whether the run uses a published NuGet package or a separately labelled source build.
- Runtime, OS, CPU, architecture, build configuration, benchmark tool/version, parameters and command.
- Raw BenchmarkDotNet artifacts, including measurements and allocation diagnostics.
- Correctness/semantic-equivalence result for the workload.
- Baseline ID and comparison method, where a baseline is meaningful.

IDs identify **what is measured**, not a promise about speed. A new algorithm or materially changed workload gets a new ID and an explicit comparison link to its predecessor. A refactor that preserves semantics may retain an ID, with the source commit recording the implementation change.

## Primitive stream baseline (historical methods retained)

| Stable ID | Current BenchmarkDotNet method | Operation / contract |
|---|---|---|
| SER-STR-001 | `ConstructionBenchmarks.SER_STR_001_MemoryStream_Construct` | Construct an empty BCL `MemoryStream` |
| SER-STR-002 | `ConstructionBenchmarks.SER_STR_002_MemoryBinaryStream_Construct` | Construct an empty package `MemoryBinaryStream` |
| SER-STR-003 | `ConstructionBenchmarks.SER_STR_003_StreamBinaryStream_Construct` | Construct package `StreamBinaryStream` over a new `MemoryStream` |
| SER-WRT-001 | `WriteBenchmarks.SER_WRT_001_MemoryStream_Write` | Create stream and write the parameterized payload |
| SER-WRT-002 | `WriteBenchmarks.SER_WRT_002_MemoryBinaryStream_Write` | Same payload write through package stream |
| SER-WRT-003 | `WriteBenchmarks.SER_WRT_003_StreamBinaryStream_Write` | Same payload write through package wrapper |
| SER-RD-001 | `ReadBenchmarks.SER_RD_001_MemoryStream_Read` | Read from read-only BCL stream into preallocated destination |
| SER-RD-002 | `ReadBenchmarks.SER_RD_002_MemoryBinaryStream_Read` | Read through package stream into same-sized destination |
| SER-RD-003 | `ReadBenchmarks.SER_RD_003_StreamBinaryStream_Read` | Read through package wrapper into same-sized destination |
| SER-CTR-001 | `ExistingDataConstructionBenchmarks.SER_CTR_001_MemoryStream_ConstructWithData` | Wrap existing data read-only without copying |
| SER-CTR-002 | `ExistingDataConstructionBenchmarks.SER_CTR_002_MemoryBinaryStream_ConstructWithData` | Construct package stream from existing data; current implementation may copy |
| SER-ARR-001 | `ToArrayBenchmarks.SER_ARR_001_MemoryStream_ToArray` | Write payload then materialize a result array |
| SER-ARR-002 | `ToArrayBenchmarks.SER_ARR_002_MemoryBinaryStream_ToArray` | Same output contract using package stream |
| SER-POS-001 | `PositionOverwriteBenchmarks.SER_POS_001_MemoryStream_PositionOverwrite` | Copy input, seek to midpoint and overwrite one byte |
| SER-POS-002 | `PositionOverwriteBenchmarks.SER_POS_002_MemoryBinaryStream_PositionOverwrite` | Same logical overwrite using package stream |
| SER-RT-001 | `SerializationContractBenchmarks.SER_RT_001_BinarySerializable_RoundTrip` | Pack length + payload, rewind, unpack and return restored length |

The legacy suite uses `Size` values 16, 1,024, 10,000 and 100,000 bytes. The stream and wrapper cases are useful historical baselines, but their construction, copying, ownership, and output contracts differ in some cases; interpret ratios only after checking the operation contract. Do not silently rewrite old cases to make newer techniques look better.

## Output-buffer strategies

These cases use the same deterministic byte payload and return its logical output length. They measure buffer strategy, not the full FSM semantic codec.

| Stable ID | Current BenchmarkDotNet method | Operation / important caveat |
|---|---|---|
| SER-BUF-001 | `OutputBufferStrategyBenchmarks.SER_BUF_001_MemoryStream_Grow` | New stream with default growth policy, then write all bytes |
| SER-BUF-002 | `OutputBufferStrategyBenchmarks.SER_BUF_002_MemoryStream_Presize` | New stream with initial capacity equal to payload size, then write all bytes |
| SER-BUF-003 | `OutputBufferStrategyBenchmarks.SER_BUF_003_ExactArray_Copy` | Allocate exact-sized output array and copy payload |
| SER-BUF-004 | `OutputBufferStrategyBenchmarks.SER_BUF_004_ArrayPool_RentCopyReturn` | Rent shared pooled buffer, copy, return without clearing |
| SER-BUF-005 | `OutputBufferStrategyBenchmarks.SER_BUF_005_ArrayPool_RentCopyClearReturn` | Same pooled operation but clear on return; measures the security-hygiene cost |

The two pool cases intentionally include rent and return on every invocation. They do **not** yet isolate a caller-managed, reused lease; add that as a separate ID only with explicit lifetime and concurrency rules. The array-copy case measures the unavoidable output allocation but does not include a stream abstraction. These are related microbenchmarks, not interchangeable end-to-end replacements.

## Planned families — reserve IDs when implementation lands

| Family | ID range | Cases to add |
|---|---|---|
| Primitive encoding | SER-INT-001… | fixed-width integers, unsigned varints, signed zigzag where appropriate, boundary values |
| Strings | SER-STRG-001… | UTF-8 length-prefixed strings, repeated strings, string-table/local-index representation |
| References and tables | SER-REF-001… | external/sparse IDs mapped to dense local indices, explicit tables, forward references |
| Layout and sizing | SER-LAY-001… | one-pass growth, exact two-pass sizing, backpatch/fixup; same canonical bytes where promised |
| FSM definitions | SER-FSM-001… | deterministic definition encode, decode, round-trip across tiny/typical/large/dense/sparse graphs |
| FSM snapshots | SER-SNP-001… | snapshot encode/decode only after snapshot consistency contract exists |
| History/replay | SER-HIS-001… | event append, history encode/decode, replay; separately from definitions |
| Mature serializers | SER-EXT-001… | benchmark-only adapters for MessagePack-CSharp, MemoryPack, protobuf-net and System.Text.Json only where a fair equivalent schema exists |
| Robustness/cost | SER-RST-001… | malformed input rejection, bounded-input cases, cold/warm setup, throughput and allocation under repeated workloads |

A planned ID is not a completed benchmark. Assign the next unused ID only when the implementation, correctness contract and setup are committed.

## Workload matrix

For every applicable family, cover payload/graph sizes and shapes rather than only one average case:

- Empty/minimal, small, medium, large and very large inputs.
- Repeated and unique strings; short and long strings.
- Sparse/high external identifiers and compact identifiers.
- Linear, branching, cyclic/reference-heavy (if the semantic model permits cycles), dense and disconnected graphs.
- Cold-start versus warmed steady state when the distinction is real.
- Valid inputs and bounded malformed inputs for decoder cost and safety.
- Encode-only, decode-only and full round-trip as separate operations.
- Throughput, latency distribution where supported, bytes written, allocated bytes, GC counts and output size.

Do not combine setup cost with the operation unless setup is part of the contract being measured. Do not compare serializers unless their schemas, fields, reference semantics, validation and output requirements are equivalent. Record deviations explicitly.

## Evidence lifecycle

1. Keep the last published-package baseline and historical artifacts immutable.
2. Run the targeted benchmark subset for each relevant change; run the full suite on a defined cadence and before release.
3. Store raw artifacts keyed by source commit, package version, runtime, machine identity and benchmark catalog revision.
4. Compare only compatible environments or mark the comparison as cross-environment/non-authoritative.
5. Use repeated runs and noise-aware thresholds; never fail a build on a single noisy timing sample.
6. Treat correctness and performance as separate gates. A faster incorrect result is a failure.
7. A future observer may trigger benchmark workflows when the serialization package changes, but should queue/deduplicate commits, record the exact package/source under test, and never publish a package or silently substitute local source for the published-package lane.
