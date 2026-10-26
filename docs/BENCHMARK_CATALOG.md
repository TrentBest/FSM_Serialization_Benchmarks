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
| `SER-RED-001` | `MemoryStream_Read` | Construct read-only stream over existing data and read into a preallocated buffer | Includes stream construction; destination buffer is reused |
| `SER-RED-002` | `MemoryBinaryStream_Read` | Construct stream over existing data and read into a preallocated buffer | Includes stream construction; destination buffer is reused |
| `SER-RED-003` | `StreamBinaryStream_Read` | Construct backing stream and wrapper, then read | Includes both stream constructions |
| `SER-DAT-001` | `MemoryStream_ConstructWithData` | Construct a read-only `MemoryStream` over existing data | Does not copy the payload |
| `SER-DAT-002` | `MemoryBinaryStream_ConstructWithData` | Construct a `MemoryBinaryStream` over existing data | Interpret ownership/copy behavior according to implementation contract |
| `SER-ARR-001` | `MemoryStream_ToArray` | Write payload then materialize via `ToArray` | Includes construction, write, and output copy |
| `SER-ARR-002` | `MemoryBinaryStream_ToArray` | Write payload then materialize via `ToArray` | Includes construction, write, and output copy |
| `SER-POS-001` | `MemoryStream_PositionOverwrite` | Copy source bytes, seek to midpoint, overwrite one byte | Includes input copy and stream construction |
| `SER-POS-002` | `MemoryBinaryStream_PositionOverwrite` | Copy source bytes, seek to midpoint, overwrite one byte | Includes input copy and stream construction |
| `SER-RT-001` | `BinarySerializable_RoundTrip` | Pack a length-prefixed payload and unpack it into a new object | Includes payload/object allocation, encoding, decoding, and stream work; this is a harness fixture, not a production codec |

## Parameters and correctness

The shared fixture generates deterministic bytes with `Data[i] = (byte)(i * 31)`. The `SER-RT-001` case now validates correctness in a targeted `GlobalSetup` before timing: restored payload length and every byte must match the input, and the stream must have no unread/trailing bytes. The timed method returns the restored length as a low-cost completion signal; the byte comparison itself is intentionally outside the measured operation.

The current code uses `MemoryStream`, `MemoryBinaryStream`, and `StreamBinaryStream` with several different setup and ownership costs. Read the timed-contract and caveat columns before comparing ratios. A fair comparison must either equalize those costs or explicitly label them as part of the operation being measured.

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
