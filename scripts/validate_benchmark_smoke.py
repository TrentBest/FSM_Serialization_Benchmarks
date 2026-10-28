#!/usr/bin/env python3
"""Validate a fresh BenchmarkDotNet smoke log and its exact case coverage.

The validator deliberately accepts only reports below the invocation's isolated
artifact directory. It never searches a shared/default BenchmarkDotNet output
folder, so stale reports cannot satisfy coverage.
"""
from __future__ import annotations

import argparse
import csv
import re
import sys
from pathlib import Path

FAILURE_PATTERNS = (
    r"BenchmarkDotNet.*\b(?:failed|error|exception)\b",
    r"Benchmark process exited with code [1-9]\d*",
    r"System\.(?:NullReferenceException|InvalidOperationException|ArgumentException|EndOfStreamException)",
    r"Exception has been thrown by the target of an invocation",
    r"(?:GlobalSetup|GlobalCleanup).*\bfailed\b",
    r"\b(?:Unhandled exception|Benchmark failed|Build Error|There are not any results to print)\b",
)

EXPECTED_CASES: dict[str, set[str]] = {}
for name in ("SER-RT-001", "SER-RED-004", "SER-RED-005"):
    EXPECTED_CASES[name] = {"Size=16", "Size=1024", "Size=10000", "Size=100000"}
for name in ("SER_INT_001", "SER_INT_002", "SER_INT_003", "SER_INT_004"):
    EXPECTED_CASES[name] = {"Distribution=Small", "Distribution=Mixed", "Distribution=Large"}
for name in (
    "SER_BUF_001", "SER_BUF_002", "SER_BUF_003", "SER_BUF_004", "SER_BUF_005",
):
    EXPECTED_CASES[name] = {"Size=16", "Size=1024", "Size=10000", "Size=100000"}


def normalized(value: object) -> str:
    return re.sub(r"[,_ ]", "", str(value or "")).lower()


def row_text(row: dict[str, str]) -> str:
    return " | ".join(f"{key}={value}" for key, value in row.items() if value is not None)


def row_matches_case(row: dict[str, str], case_id: str, parameter: str) -> bool:
    text = row_text(row)
    if case_id not in text:
        return False
    key, expected = parameter.split("=", 1)
    # BenchmarkDotNet versions may expose parameter values in a dedicated
    # column, a Params column, or in the method display name.
    for column, value in row.items():
        if column is None or value is None:
            continue
        if key.lower() in column.lower() and normalized(value) == normalized(expected):
            return True
    compact = normalized(text)
    return normalized(key + "=" + expected) in compact or normalized(key + expected) in compact


def main() -> int:
    parser = argparse.ArgumentParser()
    parser.add_argument("--log", required=True, type=Path)
    parser.add_argument("--report-root", required=True, type=Path)
    args = parser.parse_args()

    if not args.log.is_file() or args.log.stat().st_size == 0:
        print(f"ERROR: smoke log missing or empty: {args.log}", file=sys.stderr)
        return 1
    log = args.log.read_text(encoding="utf-8", errors="replace")
    for pattern in FAILURE_PATTERNS:
        match = re.search(pattern, log, re.IGNORECASE | re.MULTILINE)
        if match:
            print(f"ERROR: failure signature in smoke log: {match.group(0)}", file=sys.stderr)
            return 1

    if not args.report_root.is_dir():
        print(f"ERROR: fresh report directory missing: {args.report_root}", file=sys.stderr)
        return 1
    reports = sorted(args.report_root.rglob("*report.csv"))
    if not reports:
        print(f"ERROR: no report CSV files beneath isolated directory {args.report_root}", file=sys.stderr)
        return 1

    observed: list[tuple[Path, dict[str, str]]] = []
    for report in reports:
        try:
            with report.open(newline="", encoding="utf-8-sig") as handle:
                sample = handle.read(4096)
                handle.seek(0)
                try:
                    dialect = csv.Sniffer().sniff(sample, delimiters=",;\\t")
                except csv.Error:
                    dialect = csv.excel
                rows = list(csv.DictReader(handle, dialect=dialect))
            for row in rows:
                if row and any(str(value or "").strip() for value in row.values()):
                    observed.append((report, row))
        except (OSError, csv.Error) as exc:
            print(f"ERROR: cannot parse report {report}: {exc}", file=sys.stderr)
            return 1

    missing: list[str] = []
    for case_id, parameters in EXPECTED_CASES.items():
        for parameter in sorted(parameters):
            if not any(row_matches_case(row, case_id, parameter) for _, row in observed):
                missing.append(f"{case_id} [{parameter}]")

    if missing:
        print("ERROR: expected smoke coverage is incomplete:", file=sys.stderr)
        for case in missing:
            print(f"  - {case}", file=sys.stderr)
        print(f"Observed {len(observed)} non-empty CSV row(s) in {len(reports)} report(s).", file=sys.stderr)
        return 1

    expected_rows = sum(len(parameters) for parameters in EXPECTED_CASES.values())
    print(f"Smoke evidence validated: {expected_rows} expected case/parameter combinations; "
          f"{len(observed)} non-empty CSV row(s); {len(reports)} fresh report(s).")
    return 0


if __name__ == "__main__":
    raise SystemExit(main())
