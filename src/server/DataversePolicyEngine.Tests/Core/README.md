# DataversePolicyEngine.Core.Tests

This project contains unit tests for the core policy evaluation engine.

---

# Scope

These tests validate:

- Query filtering and ordering logic (PolicyRepository)
- Typed value comparison behavior (DataverseValueComparer)
- Operator evaluation logic (ConditionComparer)
- Policy evaluation rules:
  - Visible → first match wins
  - Required → first match wins
  - NotAllowed → deny overrides
  - AND-only conditions
  - Rule and condition sequence ordering

---

# What Is NOT Tested Here

- Dataverse plugin execution pipeline behavior
- IPluginExecutionContext behavior
- Security context behavior
- Custom API wiring
- Real Dataverse metadata validation

Those behaviors require integration testing in a Dataverse environment.

---

# Testing Strategy

- FakeXrmEasy is used to simulate Dataverse for repository behavior.
- A fake repository is used for evaluator behavior isolation.
- Sequence ordering is enforced inside the evaluator to prevent dependency on repository ordering.

---

# Design Principle

The core evaluator must be:

- Deterministic
- Independent of Dataverse plugin pipeline
- Usable by:
  - Plugin enforcement
  - Custom API system-context evaluation

If evaluator behavior changes, update both unit tests and architecture documentation.