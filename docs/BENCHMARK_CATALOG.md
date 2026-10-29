<!-- The Singularity Workshop moniker -->
<p align="center">
  <a href="https://github.com/TrentBest">
    <img src="https://raw.githubusercontent.com/TrentBest/TheSingularityWorkshop.Visualize/development/docs/assets/moniker.svg" alt="The Singularity Workshop — animated workshop moniker" width="720" />
  </a>
</p>

# ✳️ FSM_Serialization_Benchmarks — Benchmark Catalog

This catalog defines the measured operation, stable identity, and interpretation limits of each currently implemented BenchmarkDotNet case. IDs identify a workload contract; they do not imply equivalence between different implementations or promise a particular performance result.

This document follows the [Workshop Documentation Standard](https://github.com/TrentBest/TheSingularityWorkshop.FSM_COS/blob/master/DOCUMENTATION_STANDARD.md). Start with the [Living Gallery](GALLERY.md) for a visual explanation of how correctness gates performance interpretation, then use this catalog for exact workload contracts.

## Identity rules

- Format: `SER-<FAMILY>-<NNN>`.
- Keep an ID when its semantic contract remains unchanged; do not reuse retired IDs.
- A material change to what is timed requires a new ID or an explicitly documented contract revision.
- BenchmarkDotNet parameters are part of the workload identity in a result row. Stream and buffer size parameters are 16, 1,024, 10,000, and 100,000 bytes; integer cases use Small, Mixed, and Large distributions of 4,096 UInt32 values each.
- Correctness is a separate gate from speed. Preserve raw BenchmarkDotNet reports and the exact source/harness commit and environment.
- These are stream/primitive-contract benchmarks, not evidence for a production FSM definition codec.

## Current cases

| Stable ID | Benchmark method | Timed contract | Important caveat |
|---|---|---|---|
| `SER-STR-001` | `MemoryStream_Construct` | Construct an empty `MemoryStream` | Construction only; no payload work |
| `SER-STR-002` | `MemoryBinaryStream_Construct` | Construct an empty `MemoryBinaryStream` | Construction only; no payload work |
| `SER-STR-003` | `StreamBinaryStream_Construct` | Construct a `StreamBinaryStream` with a new `MemoryStream` | Includes backing-stream construction |
| `SER-WRT-001` | `MemoryStream_Write` | Construct stream, write the parameterized byte array, return stream | Includes construction and write; returned stream is not disposed in the method |
| `SER-WRT-002` | `MemoryBinaryStream_Write` | Construct stream, write the parameterized byte array, return stream | Includes construction and write; returned stream is not disposed in the method |
| `SER-WRT-003` | `StreamBinaryStream_Write` | Construct wrapper and backing stream, write payload, return wrapper | Includes wrapper/backing construction and write |
| `SER-RED-001` | `MemoryStream_Read` | Construct read-only stream over existing data and read into a preallocated buffer | Includes construction and reads without copying the input payload; end-to-end case, not an isolated read call |
| `SER-RED-002` | `MemoryBinaryStream_Read` | Construct stream over existing data and read into a preallocated buffer | Includes construction and an input payload copy before reading; compare as end-to-end cost, not isolated read cost |
| `SER-RED-003` | `StreamBinaryStream_Read` | Construct backing stream and wrapper, then read | Includes both stream constructions; end-to-end case, not an isolated read call |
| `SER-RED-004` | `MemoryStream_ReadPreparedPayload` | Reset position and read a prepared payload into a reused buffer | Construction and payload setup occur outside timing; position reset is included |
| `SER-RED-005` | `MemoryBinaryStream_ReadPreparedPayload` | Reset position and read the same prepared payload shape into a reused buffer | Construction and payload copy occur outside timing; position reset is included; compare with `SER-RED-004` for read-path overhead |
| `SER-DAT-001` | `MemoryStream_ConstructWithData` | Construct a read-only `MemoryStream` over existing data | Wraps the caller's array without copying it; ownership and writability semantics differ from the candidate |
| `SER-DAT-002` | `MemoryBinaryStream_ConstructWithData` | Construct a `MemoryBinaryStream` from existing data | Copies bytes into an expandable internal `MemoryStream`; the baseline wraps existing storage, so this is an intentional copy-vs-wrap comparison, not equivalent construction work |
| `SER-ARR-001` | `MemoryStream_ToArray` | Write payload then materialize via `ToArray` | Includes construction, write, and output copy |
| `SER-ARR-002` | `MemoryBinaryStream_ToArray` | Write payload then materialize via `ToArray` | Includes construction, write, and output copy |
| `SER-POS-001` | `MemoryStream_PositionOverwrite` | Copy source bytes, seek to midpoint, overwrite one byte | Includes input copy and stream construction |
| `SER-POS-002` | `MemoryBinaryStream_PositionOverwrite` | Copy source bytes, seek to midpoint, overwrite one byte | Includes input copy and stream construction |
| `SER-RT-001` | `BinarySerializable_RoundTrip` | Pack a length-prefixed payload and unpack it into a new object | Includes payload/object allocation, encoding, decoding, and stream work; this is a harness fixture, not a production codec |
| `SER-BUF-001` | `SER_BUF_001_MemoryStream_Grow` | Write the payload to a growing `MemoryStream` | Includes stream construction, growth, write, and disposal |
| `SER-BUF-002` | `SER_BUF_002_MemoryStream_Presize` | Write the payload to a pre-sized `MemoryStream` | Includes stream construction, write, and disposal; capacity differs from the growing case |
| `SER-BUF-003` | `SER_BUF_003_ExactArray_Copy` | Allocate an exact-size array and copy payload bytes | Measures allocation plus copy, not a stream API |
| `SER-BUF-004` | `SER_BUF_004_ArrayPool_RentCopyReturn` | Rent pooled storage, copy bytes, return storage | Pool rent/return costs and oversized rented arrays are part of the strategy |
| `SER-BUF-005` | `SER_BUF_005_ArrayPool_RentCopyClearReturn` | Rent pooled storage, copy bytes, clear and return storage | Includes clearing cost on return |
| `SER-INT-001` | `SER_INT_001_FixedWidthUInt32` | Encode 4,096 UInt32 values as little-endian fixed-width integers | Output array allocation is timed; the buffer is sized to fixed-width maximum |
| `SER-INT-002` | `SER_INT_002_Uleb128UInt32` | Encode the same values as canonical ULEB128 | Output array allocation and encoding are timed; local ULEB128 is not a production contract |
| `SER-INT-003` | `SER_INT_003_FixedWidthUInt32_Decode` | Decode the prepared fixed-width payload | Payload setup and correctness checks are outside timing |
| `SER-INT-004` | `SER_INT_004_Uleb128UInt32_Decode` | Decode the prepared canonical ULEB128 payload | Payload setup and malformed-input checks are outside timing |

## Parameters and correctness

The shared fixture generates deterministic bytes with `Data[i] = (byte)(i * 31)`. The `SER-RT-001` case now validates correctness in a targeted `GlobalSetup` before timing: restored payload length and every byte must match the input, and the stream must have no unread/trailing bytes. The timed method returns the restored length as a low-cost completion signal; the byte comparison itself is intentionally outside the measured operation.

The current code uses `MemoryStream`, `MemoryBinaryStream`, and `StreamBinaryStream` with several different setup and ownership costs. In particular, the original `SER-RED-001` through `SER-RED-003` cases time construction plus reading; `SER-RED-002` includes copying the entire input payload because `MemoryBinaryStream(byte[])` copies into an expandable stream, whereas `MemoryStream(byte[], writable: false)` wraps the caller's array. Do not interpret their ratio as isolated `Read`-method overhead. Use `SER-RED-004` versus `SER-RED-005` for a prepared-stream comparison, while remembering that the candidate's internal stream and baseline's fixed backing array are not identical storage implementations. `SER-DAT-001` versus `SER-DAT-002` explicitly measures wrap-vs-copy semantics, not equivalent construction work.

## Current development coverage

The current `development` harness contains the stream cases above plus `SER-BUF-001..005` and `SER-INT-001..004`. These are implemented benchmark families, but their presence in source is not proof of a successful run; CI must verify the intended case count and correctness gates before calling smoke execution successful.

## Planned families — not yet measured

- Signed integer encodings such as zigzag, plus dedicated integer round-trip and payload-size reporting.
- String encode/decode, including empty, short, typical, and large values.
- Reference/table operations, layout and size calculation.
- Concrete FSM definition workloads only after the format owner defines a real codec and compatibility contract.
- Snapshot and history/replay workloads only when their serialization semantics are owned and specified.
- Malformed-input and resource-limit cases where the format has meaningful rejection guarantees.
- Equivalent-semantics adapters for external serializer comparisons.

Do not claim these planned families are implemented merely because they appear here.

## Integer fixture and buffer strategy notes

Integer encode and decode cases share deterministic Small, Mixed, and Large sequences of 4,096 UInt32 values. Each sequence repeats an edge-rich pattern to amortize invocation overhead while retaining values near ULEB128 width boundaries and UInt32 extremes. The same distribution produces identical values for fixed-width and ULEB128 comparisons. Payload preparation and correctness checks are outside the timed decode methods; encode output allocation remains timed.

The ULEB128 helper requires shortest-form canonical encodings and checks truncated input, UInt32 overflow, excess continuation bytes, and overlong encodings of zero and one. This is a benchmark-local contract; it does not claim that the production serializer implements or guarantees ULEB128.

Buffer cases deliberately measure different allocation strategies rather than claiming every operation is semantically identical: the pool cases include rent/copy/return in the timed operation, and the clear-on-return case includes the additional clearing cost. Interpret their results as strategy comparisons, not as a drop-in production replacement.

## Result interpretation

A workflow pass means the configured smoke workload count was reached, the smoke log passed failure checks, correctness gates passed, and reports were produced. The current `development` host additionally evaluates repeated controlled evidence through CiAi. In [run #40792971412](https://github.com/TrentBest/FSM_Serialization_Benchmarks/actions/runs/40792971412), five independent published-package launches and five exact-source launches were compared separately for each `SER-RT-001` size. CiAi returned `passed` for all four workloads: Size 16 (103.9 ns vs 103.9 ns, 0.00%), 1,024 (377.7 ns vs 361.6 ns, -4.26%), 10,000 (2,454.9 ns vs 2,211.4 ns, -9.92%), and 100,000 (59,986.9 ns vs 59,012.4 ns, -1.62%). The run also verified the deterministic regression fixture (+20% median) returns `regression`. These results apply only to this harness round-trip workload and recorded environment; they are not a domain-completeness result, broad serializer-performance claim, or release authorization. Compare equivalent cases under recorded runtime/hardware conditions, retain raw artifacts, and use repeated evidence before classifying a regression. Compare equivalent cases under recorded runtime/hardware conditions, retain raw artifacts, and use repeated evidence before classifying a regression.

## Related resources

- [Living Gallery](GALLERY.md)
- [Benchmark README](../README.md)
- [Documentation coverage inventory](DOCUMENTATION_INVENTORY.md)
- [FSM_Serialization source](https://github.com/TrentBest/TheSingularityWorkshop.FSM_Serialization)
- [FSM_Serialization domain completeness review](https://github.com/TrentBest/TheSingularityWorkshop.FSM_Serialization/blob/development/docs/DOMAIN_COMPLETENESS.md)
- [Shared Workshop documentation standard](https://github.com/TrentBest/TheSingularityWorkshop.FSM_COS/blob/master/DOCUMENTATION_STANDARD.md)
- [Workshop GitHub](https://github.com/TrentBest)
- [Support the Workshop](https://www.patreon.com/TheSingularityWorkshop)

---

<!-- Canonical Avatar.jpg footer still requires a verified repository asset path; do not substitute another image or invent a URL. -->
