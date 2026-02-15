# DataversePolicyEngine.Core

Core evaluation library for the Dataverse Policy Engine (DPE). This library contains the deterministic policy evaluator, repository accessors, and value/comparison helpers used by both the server plugin and the Custom API so client and server share identical policy semantics.

## Purpose
Provide a single, well-tested evaluation engine for:
- Determining `Visible` / `Required` / `NotAllowed` decisions for a specific attribute.
- Loading policy rules and ordered conditions from Dataverse.
- Performing type-aware comparisons (string, number, boolean, option set, lookup).

The core library is intentionally independent of the plugin execution pipeline so it can be reused by multiple entry points.

## Target
Projects in this repository targeting the core library compile for `.NET Framework 4.6.2`. Preserve this target when modifying the project unless intentionally migrating.

## Public API
- `IPolicyEvaluator`
  - Method: `PolicyDecision EvaluateAttribute(IOrganizationService service, string entityLogicalName, string attributeLogicalName, Entity target, Entity preImage)`
  - Returns effective decisions for the specified attribute using the provided `IOrganizationService` to load active rules/conditions.

## Key classes / files
- `Evaluation\PolicyEvaluator.cs` — Implements `IPolicyEvaluator`. Orchestrates evaluation for `Visible`, `Required`, and `NotAllowed`.
- `Data\PolicyRepository.cs` — Dataverse query logic to load active `dpe_policyrule` and ordered `dpe_policycondition` records.
- `Evaluation\Comparers\DataverseValueComparer.cs` — Typed comparison helpers used by condition evaluation (string, number, boolean, option set, lookup).
- `Evaluation\Comparers\ConditionComparer.cs` — Operator logic (Equals, NotEquals, IsNull, IsNotNull) used to evaluate conditions.
- `Model\PolicyDecision.cs` — Result object carrying `Visible`, `Required`, `NotAllowed` and matched rule ids (for diagnostics).

## Evaluation semantics (summary)
- `Visible` — first matching rule wins (default `true`).
- `Required` — first matching rule wins (default `false`).
- `NotAllowed` — deny-overrides: any matching rule with `Result = true` blocks (default `false`).
- Conditions are ANDed and evaluated in `dpe_sequence` order. Rules are ordered by `dpe_sequence`.

## Example usage

```c#
// IOrganizationService service = ... (caller supplies)

var repo = new DataversePolicyEngine.Core.Data.PolicyRepository();
var evaluator = new DataversePolicyEngine.Core.Evaluation.PolicyEvaluator(repo);

Entity target = new Entity("account"); // values being written
Entity preImage = null; // or pre-image from plugin context

var decision = evaluator.EvaluateAttribute(service, "account", "name", target, preImage);

// decision.Visible, decision.Required, decision.NotAllowed
```

## Build & test
- Open the solution in Visual Studio 2026.
- Restore NuGet packages and build the solution (projects target .NET Framework 4.6.2).
- Unit tests for the core library live in `src/server/DataversePolicyEngine.Tests/Core` and use FakeXrmEasy for repository-level tests and isolated evaluator tests.

## Deployment / Signing
- `DataversePolicyEngine.Core` is signed in-repo (strong-name) because deployable projects (plugin) depend on it. Preserve signing (`dpe.snk`) when modifying project properties.

## Contributing / Extensions
- Implement evaluator changes here first to keep server and client behavior consistent.
- Add unit tests under `DataversePolicyEngine.Tests/Core` to protect semantics (ordering, typed comparisons, operator behavior).
- Avoid coupling core to plugin execution context — keep methods testable with a mocked `IOrganizationService`.

## References
- `docs/architecture/PolicyEngine-v1.md` — architecture, data model, evaluation rules.
- `src/server/DataversePolicyEngine.Plugin/PolicyEnforcementPlugin.cs` — example consumer of `IPolicyEvaluator`.
- `src/server/DataversePolicyEngine.Tests/Core/` — tests that validate core behavior.