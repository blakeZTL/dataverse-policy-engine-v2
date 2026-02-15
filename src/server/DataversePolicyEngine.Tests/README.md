# DataversePolicyEngine.Tests

Server-side unit and integration-style tests for the Dataverse Policy Engine. Tests use MSTest and FakeXrmEasy to validate the shared evaluator, repository queries, comparers, and the plugin enforcement logic in isolation.

## Purpose
- Verify deterministic evaluation semantics (Visible / Required / NotAllowed).
- Validate `PolicyRepository` queries and condition ordering.
- Exercise `PolicyEnforcementPlugin` behavior (Create/Update) using FakeXrmEasy plugin execution.
- Provide fast, repeatable verification during development.

## Frameworks / Tools
- Test framework: `MSTest`
- Dataverse test double: `FakeXrmEasy`
- Run tests from Visual Studio 2026 via __Test Explorer__.

## Test structure
- `DataversePolicyEngine.Tests/Core` — unit tests for the core library:
  - `PolicyEvaluatorTests.cs` — evaluation ordering and semantics.
  - Comparer tests: `DataverseValueComparerTests.cs`, `ConditionComparer` tests.
  - Uses lightweight mocks or `FakeXrmEasy` for repository behavior.
- `DataversePolicyEngine.Tests/Plugin` — plugin-focused tests:
  - `PolicyEnforcementPluginTests.cs` — exercises Create and Update flows, PreImage handling, and exception messages.
  - Tests use a shared `FakeXrmEasyTestBase` that initializes an `XrmFakedContext` and supplies plugin contexts.

Key test file examples:
- `src/server/DataversePolicyEngine.Tests/Plugin/PolicyEnforcementPluginTests.cs`
- `src/server/DataversePolicyEngine.Tests/Core/...` (comparer and evaluator tests)

## How to run tests
1. Open the solution in Visual Studio 2026.
2. Restore packages (use __NuGet Package Manager__ or right-click solution → __Restore NuGet Packages__).
3. Build the solution.
4. Run tests from __Test Explorer__ (run all, group, or run individual tests).

Tips:
- Use Test Explorer filtering to run specific test classes.
- When debugging a failing test, set a breakpoint in the evaluator or plugin and run the test under the debugger.
- `FakeXrmEasy` provides context initialization (`ctx.Initialize(...)`) — seed `dpe_policyrule` and `dpe_policycondition` test data specifically for the scenario.

## Adding tests
- Prefer adding tests to cover core semantics in `Tests/Core` first — changes to `DataversePolicyEngine.Core` should be validated there.
- For plugin behavior add/extend tests in `Tests/Plugin` and reuse `FakeXrmEasyTestBase` for consistent setup.
- Keep tests deterministic: explicitly set `dpe_sequence`, `statecode`, and typed value columns for conditions (e.g., `dpe_valueoptionsetvalue`).

## Notes
- Projects target `.NET Framework 4.6.2` — run and debug tests in Visual Studio (CLI `dotnet test` is not applicable for this target).
- Tests intentionally avoid Dataverse integration; use them to catch logic regressions quickly before any manual integration testing.

## References
- `src/server/DataversePolicyEngine.Core/README.md` — core behavior to validate.
- `src/server/DataversePolicyEngine.Plugin/README.md` — plugin registration and enforcement expectations.
- `src/server/DataversePolicyEngine.Tests/Plugin/PolicyEnforcementPluginTests.cs` — example plugin tests.