# Phase 6 - Bugfixing Report (PermissionChecker)

This document records all fixes and root causes discovered while implementing and testing Phase 6 (permissions) in SlotOS on Cosmos OS.

---

## Summary
- Fixed multiple Cosmos-incompatible string operations causing CPU Invalid Opcode 06.
- Replaced incorrect console API usages.
- Stabilized tests by avoiding APIs known to be unsafe in Cosmos.
- Adjusted test expectations to match simplified, Cosmos-safe output.

---

## Root Causes (Cosmos constraints)
- String interpolation: `$"...{var}..."` can trigger opcode 06 in Cosmos.
- String concatenation with `+=` across many lines is unstable.
- StringBuilder is not supported and causes opcode 06.
- Some string helpers like `string.Contains`, `StartsWith`, `EndsWith` can crash.
- VFS is disabled in SlotOS (In-Memory mode) per earlier decision; irrelevant for Phase 6 but noted.

References (examples checked online):
- Cosmos examples use `System.Console.WriteLine(...)` directly.
- Known community feedback: avoid complex string operations in kernel space; prefer simple concatenation and `IndexOf` checks.

---

## Fixes Applied

### 1) Console API usage in tests
- File: `SlotOS/SlotOS/SlotOS/system/PermissionCheckerTest.cs`
- Problem: Calls to `Sys.Console.WriteLine()` (does not exist). Build errors CS0117.
- Fix: Use `System.Console.WriteLine()` (i.e., `Console.WriteLine()`).
- Also added: `using Sys = Cosmos.System;` only for Cosmos types (not Console).

### 2) Unsafe string search in tests
- File: `SlotOS/SlotOS/SlotOS/system/PermissionCheckerTest.cs`
- Problem: `summary.Contains("...")` caused CPU opcode 06 near Permission Summary tests.
- Fix: Replace with `summary.IndexOf("...") >= 0` in all 5 locations.

### 3) Unsafe string interpolation and multi-line concatenation in implementation
- File: `SlotOS/SlotOS/SlotOS/system/PermissionChecker.cs`
- Method: `PermissionChecker.GetPermissionSummary(User user)`
- Problems:
  - Used string interpolation and `+=` to build a long multi-line summary.
  - Crashed under Cosmos (opcode 06).
- Fix:
  - Rewrote method to return a short, fixed string per role without interpolation or `+=`.
  - Returned values:
    - Admin: "Administrator-Rechte: Volle Kontrolle uber alle Systemfunktionen und Dateien"
    - Standard: "Standard-Benutzer: Zugriff auf eigenes Home-Verzeichnis und offentliche Dateien"
    - Guest: "Gast: Eingeschrankte Berechtigungen, nur Lesezugriff"
    - Inactive: handled before by early return "Keine Berechtigungen (Konto deaktiviert)"
    - Null user: handled by early return "Keine Berechtigungen (kein Benutzer)"

### 4) String interpolation in error paths
- File: `SlotOS/SlotOS/SlotOS/system/PermissionChecker.cs`
- Method: `RequirePermission(User user, string action)`
- Problem: `DenyAccess($"Sie haben keine Berechtigung fur: {actionName}")`
- Fix: `DenyAccess("Sie haben keine Berechtigung fur: " + actionName)`

### 5) String interpolation in test output
- File: `SlotOS/SlotOS/SlotOS/system/PermissionCheckerTest.cs`
- Problems:
  - `$"ALLE {_totalTests} TESTS BESTANDEN!"`, `$"... {_testsFailed} ..."`, `$"[OK] {testName}"`, `$"[FAIL] {testName}"`, `$"[ERROR] {testName}: {ex.Message}"`.
- Fixes:
  - Replace all with simple concatenation and/or static text.
  - Example: `Console.WriteLine("[OK] " + testName);`
  - Summary lines simplified (print counters separately with `ToString()`).

---

## Test Adjustments
- File: `SlotOS/SlotOS/SlotOS/system/PermissionCheckerTest.cs`
- After simplifying `GetPermissionSummary(...)` outputs, updated expectations:
  - Admin: check `IndexOf("Administrator-Rechte") >= 0`
  - Standard: check `IndexOf("Standard-Benutzer") >= 0`
  - Guest: check `IndexOf("Gast") >= 0`
  - Inactive: check `IndexOf("Konto deaktiviert") >= 0`
  - Null user: unchanged ("kein Benutzer")

---

## Build/Run Issues Observed
- IL2CPU file lock: `SlotOS.cdb` in `bin/Debug/net6.0` sometimes locked by running VM.
  - Resolution: stop debugging, close VM, Clean Solution, rebuild.

---

## Validation Steps
1) Clean and Rebuild Solution.
2) Boot SlotOS in emulator.
3) Run Phase 6 tests via command (e.g. `testp6` / `testpermissions`).
4) Ensure no opcode 06 appears; all tests complete.

Observed result: Opcode 06 exceptions on Permission Summary resolved; test suite completes. Build errors due to Console API fixed.

---

## Best Practices (Cosmos-safe)
- Use `Console.WriteLine` from `System` only; do not use `Sys.Console`.
- Avoid `$"..."` interpolation; prefer simple `"a" + b` when needed.
- Avoid `string +=` in loops or for multi-line building; return fixed strings where possible.
- Avoid `Contains`, `StartsWith`, `EndsWith`; use `IndexOf(...) >= 0`.
- Do not use `StringBuilder`.
- Keep outputs ASCII-only.

---

## Affected Files
- `SlotOS/SlotOS/SlotOS/system/PermissionChecker.cs`
- `SlotOS/SlotOS/SlotOS/system/PermissionCheckerTest.cs`

---

## Next Steps
- Keep summaries short and constant to reduce string ops in kernel.
- Extend tests for permission enforcement without heavy string processing.
- If future text output grows, consider splitting into multiple `Console.WriteLine` calls with constants.
