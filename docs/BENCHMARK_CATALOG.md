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
- BenchmarkDotNet parameters are part of the workload identity in a result row. The current size parameter is 16, 1,024, 10,000, and 100,000 bytes.
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

## Parameters and correctness

The shared fixture generates deterministic bytes with `Data[i] = (byte)(i * 31)`. The `SER-RT-001` case now validates correctness in a targeted `GlobalSetup` before timing: restored payload length and every byte must match the input, and the stream must have no unread/trailing bytes. The timed method returns the restored length as a low-cost completion signal; the byte comparison itself is intentionally outside the measured operation.

The current code uses `MemoryStream`, `MemoryBinaryStream`, and `StreamBinaryStream` with several different setup and ownership costs. In particular, the original `SER-RED-001` through `SER-RED-003` cases time construction plus reading; `SER-RED-002` includes copying the entire input payload because `MemoryBinaryStream(byte[])` copies into an expandable stream, whereas `MemoryStream(byte[], writable: false)` wraps the caller's array. Do not interpret their ratio as isolated `Read`-method overhead. Use `SER-RED-004` versus `SER-RED-005` for a prepared-stream comparison, while remembering that the candidate's internal stream and baseline's fixed backing array are not identical storage implementations. `SER-DAT-001` versus `SER-DAT-002` explicitly measures wrap-vs-copy semantics, not equivalent construction work.

## Planned families — not yet measured

- Integer fixed-width/variable-length encode and decode, including signed zigzag and full round-trip cases.
- String encode/decode, including empty, short, typical, and large values.
- Reference/table operations, layout and size calculation.
- Concrete FSM definition workloads only after the format owner defines a real codec and compatibility contract.
- Snapshot and history/replay workloads only when their serialization semantics are owned and specified.
- Malformed-input and resource-limit cases where the format has meaningful rejection guarantees.
- Equivalent-semantics adapters for external serializer comparisons.

Do not claim these planned families are implemented merely because they appear here.

## Result interpretation

A workflow pass means configured workloads executed and reports were produced. It is not a performance win, a regression verdict, a domain-completeness result, or release authorization. Compare equivalent cases under recorded runtime/hardware conditions, retain raw artifacts, and use repeated evidence before classifying a regression.

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

<!-- Canonical Avatar.jpg footer not yet applied: no verified Avatar.jpg asset path was found. Do not substitute another image or invent a URL. -->
