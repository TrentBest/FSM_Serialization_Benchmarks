"""Regression tests for the BenchmarkDotNet smoke evidence gate."""
from __future__ import annotations

import csv
import subprocess
import sys
import tempfile
import unittest
from pathlib import Path

VALIDATOR = Path(__file__).resolve().parent / "validate_benchmark_smoke.py"


class SmokeValidatorTests(unittest.TestCase):
    def setUp(self) -> None:
        self.temp = tempfile.TemporaryDirectory()
        self.root = Path(self.temp.name)
        self.log = self.root / "smoke.log"
        self.reports = self.root / "fresh-artifacts" / "results"
        self.reports.mkdir(parents=True)

    def tearDown(self) -> None:
        self.temp.cleanup()

    def run_validator(self) -> subprocess.CompletedProcess[str]:
        return subprocess.run(
            [sys.executable, str(VALIDATOR), "--log", str(self.log),
             "--report-root", str(self.root / "fresh-artifacts")],
            text=True, capture_output=True, check=False,
        )

    def write_complete_report(self, delimiter: str = ",") -> None:
        cases: list[tuple[str, str, str]] = []
        sizes = ("16", "1024", "10000", "100000")
        distributions = ("Small", "Mixed", "Large")
        for case_id in ("SER-RT-001", "SER-RED-004", "SER-RED-005"):
            cases.extend((case_id, size, "") for size in sizes)
        for case_id in ("SER_INT_001", "SER_INT_002", "SER_INT_003", "SER_INT_004"):
            cases.extend((case_id, "", distribution) for distribution in distributions)
        for case_id in ("SER_BUF_001", "SER_BUF_002", "SER_BUF_003", "SER_BUF_004", "SER_BUF_005"):
            cases.extend((case_id, size, "") for size in sizes)

        path = self.reports / "smoke-report.csv"
        with path.open("w", newline="", encoding="utf-8") as handle:
            writer = csv.DictWriter(handle, fieldnames=["Method", "Size", "Distribution", "Mean"], delimiter=delimiter)
            writer.writeheader()
            for case_id, size, distribution in cases:
                writer.writerow({"Method": case_id, "Size": size,
                                 "Distribution": distribution, "Mean": "1.0 ns"})

    def test_complete_fresh_coverage_passes(self) -> None:
        self.log.write_text("Global total time: 00:00:01, executed benchmarks: 44\n", encoding="utf-8")
        self.write_complete_report()
        result = self.run_validator()
        self.assertEqual(result.returncode, 0, result.stdout + result.stderr)
        self.assertIn("44 expected case/parameter combinations", result.stdout)

    def test_tab_delimited_fresh_coverage_passes(self) -> None:
        self.log.write_text("Global total time: 00:00:01, executed benchmarks: 44\\n", encoding="utf-8")
        self.write_complete_report(delimiter="\\t")
        result = self.run_validator()
        self.assertEqual(result.returncode, 0, result.stdout + result.stderr)
        self.assertIn("44 expected case/parameter combinations", result.stdout)

    def test_exception_in_zero_exit_log_fails(self) -> None:
        # This reproduces the historic failure mode: the process can exit zero
        # even though BenchmarkDotNet reported a selected case exception.
        self.log.write_text(
            "Global total time: 00:00:01, executed benchmarks: 44\n"
            "System.NullReferenceException: injected regression fixture\n",
            encoding="utf-8",
        )
        self.write_complete_report()
        result = self.run_validator()
        self.assertNotEqual(result.returncode, 0)
        self.assertIn("NullReferenceException", result.stderr)

    def test_missing_parameter_row_fails(self) -> None:
        self.log.write_text("Global total time: 00:00:01, executed benchmarks: 44\n", encoding="utf-8")
        self.write_complete_report()
        report = self.reports / "smoke-report.csv"
        rows = list(csv.DictReader(report.open(encoding="utf-8", newline="")))
        rows = [row for row in rows if not (row["Method"] == "SER_BUF_005" and row["Size"] == "100000")]
        with report.open("w", newline="", encoding="utf-8") as handle:
            writer = csv.DictWriter(handle, fieldnames=["Method", "Size", "Distribution", "Mean"])
            writer.writeheader()
            writer.writerows(rows)
        result = self.run_validator()
        self.assertNotEqual(result.returncode, 0)
        self.assertIn("SER_BUF_005", result.stderr)
        self.assertIn("Size=100000", result.stderr)

    def test_default_or_stale_report_outside_isolated_root_is_ignored(self) -> None:
        self.log.write_text("Global total time: 00:00:01, executed benchmarks: 44\n", encoding="utf-8")
        stale = self.root / "old" / "report.csv"
        stale.parent.mkdir()
        stale.write_text("Method,Size,Mean\nSER-RT-001,16,1 ns\n", encoding="utf-8")
        result = self.run_validator()
        self.assertNotEqual(result.returncode, 0)
        self.assertIn("no report CSV files beneath isolated directory", result.stderr)


if __name__ == "__main__":
    unittest.main()
