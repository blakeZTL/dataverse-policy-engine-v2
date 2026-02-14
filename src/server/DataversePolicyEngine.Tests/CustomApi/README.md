# DataversePolicyEngine.Tests

This project contains unit and simulated integration tests for:

- DataversePolicyEngine.Core
- DataversePolicyEngine.Plugin
- DataversePolicyEngine.CustomApi

Frameworks:

- MSTest
- FakeXrmEasy

---

# Testing Strategy

The Policy Engine uses a layered testing approach:

1. Core logic tests (pure evaluation)
2. Plugin simulation tests (server enforcement behavior)
3. Custom API simulation tests (system-context evaluation)

Real Dataverse validation is performed separately.

---

# Custom API Tests

The Custom API tests validate:

- Correct Visible/Required/NotAllowed outputs
- Required returns true when matching rule applies
- NotAllowed returns true when matching rule applies
- Default values returned when no rules match
- Typed trigger values:
  - String
  - Number
  - Boolean
  - OptionSet
  - Lookup
- Rule sequence ordering respected
- AND-only condition behavior respected

---

# Plugin Tests

Plugin tests validate:

- NotAllowed blocks Update when value changes
- NotAllowed does not block when value unchanged
- Required blocks Update when value set to null
- Required blocks Create when attribute missing/null
- NotAllowed blocks Create when attribute set
- Enforcement respects trigger conditions
- Enforcement respects sequence ordering

These tests simulate:

- Create pipeline
- Update pipeline
- PreImage behavior

---

# What Is NOT Tested Here

- Custom API registration correctness in Dataverse
- Plugin step registration correctness
- Real Dataverse transaction pipeline behavior
- Security role configuration
- Performance under production load
- Client-side integration behavior

These require manual integration testing in a Dataverse environment.

---

# Test Philosophy

The engine is designed to be:

- Deterministic
- Order-aware
- Side-effect free
- Reusable across:
  - Plugin enforcement
  - Custom API evaluation
  - Future client integrations

If evaluation behavior changes:

1. Update Core tests first.
2. Update Plugin tests.
3. Update Custom API tests.
4. Update documentation.

Tests define the behavioral contract of the engine.
