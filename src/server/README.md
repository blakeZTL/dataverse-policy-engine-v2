# Dataverse Policy Engine — Server Overview

This folder contains the server-side pieces of the Dataverse Policy Engine (DPE) solution: the enforcement plugin, the Custom API used for system-context evaluation, and the server-focused tests. The server projects provide authoritative enforcement and the in-process evaluation engine used by all entry points.

## What lives here
- `DataversePolicyEngine.Plugin` — synchronous PreOperation plugin that enforces `Required` and `NotAllowed` policies inside the Dataverse transaction.
  - Assembly: `DataversePolicyEngine.Plugin`
  - Main class: `DataversePolicyEngine.Plugin.PolicyEnforcementPlugin`
- `DataversePolicyEngine.CustomApi` — Custom API (`dpe_EvaluatePolicies`) that evaluates policies in system context for client JS usage.
- `DataversePolicyEngine.Tests` — MSTest + FakeXrmEasy tests that validate plugin and evaluator behavior.
- Shared evaluator (project lives in the solution root): `DataversePolicyEngine.Core` — policy loading and evaluation logic used by both plugin and Custom API.

## Key responsibilities
- Plugin enforces:
  - Required: Create/Update checks that required fields are present or not cleared.
  - NotAllowed: Blocks sets/changes when a rule denies the action.
- Custom API returns deterministic decisions (Visible / Required / NotAllowed) for the form UI to apply.
- Core contains the single evaluation engine so client and server use identical semantics.

## Registration (exact settings)
Register plugin steps per governed entity:

Create step
- Message: `Create`
- Stage: `PreOperation`
- Mode: `Synchronous`

Update step
- Message: `Update`
- Stage: `PreOperation`
- Mode: `Synchronous`
- PreImage:
  - Name: `PreImage` (required by evaluator)
  - Attributes: include governed attributes (or use All Attributes while testing)

Notes
- The update PreImage must be named exactly `PreImage` for full enforcement behaviour.
- The plugin evaluates only attributes that have at least one active policy rule for the target entity.

## Build, test and local development
1. Open the solution in Visual Studio 2026.
2. Restore packages: use __NuGet Package Manager__ or right-click the solution → __Restore NuGet Packages__.
3. Build: use __Build Solution__.
4. Run unit tests from __Test Explorer__. Tests use MSTest and FakeXrmEasy for Dataverse isolation.

## Deployment notes
- Assemblies intended for Dataverse plugin deployment must be strong-named. The repo contains `dpe.snk`; projects intended for deployment are configured to sign with that key. Preserve signing if you change project properties.
- The plugin throws `InvalidPluginExecutionException` to block offending operations; use `ITracingService` to surface helpful diagnostics during registration or troubleshooting.

## Troubleshooting
- Enforcement not firing: confirm plugin steps are registered exactly as above and the Update step includes the named `PreImage`.
- Unexpected allow/deny: confirm `dpe_policyrule` and `dpe_policycondition` records are `Active`, and `TargetEntityLogicalName` / `TargetAttributeLogicalName` match the schema logical names.
- Use the unit tests (FakeXrmEasy) to reproduce evaluation scenarios locally and to validate any evaluator changes.

## Where to look next
- `docs/help.html` — builder guide for wiring the client form and example `config=` parameter.
- `docs/architecture/PolicyEngine-v1.md` — detailed architecture, data model, and evaluation semantics.
- `src/server/DataversePolicyEngine.Plugin/README.md` — plugin-specific details and examples.
