# Dataverse Policy Engine (DPE)

A small, deterministic engine to define and enforce attribute-level policies in Microsoft Dataverse. DPE provides:
- Client-side evaluation (JavaScript + Custom API) for UI behavior (visibility, required, disable).
- Server-side enforcement (synchronous PreOperation plugin) for authoritative checks (Required, NotAllowed).
- A shared evaluation engine so client and server use the same policy logic.

See detailed architecture and builder guidance in `docs/architecture` and `docs/help.html`.

## Key Concepts
- PolicyRule (`dpe_policyrule`): header record that targets a single attribute and a single `PolicyType` (Visible / Required / NotAllowed).
- PolicyCondition (`dpe_policycondition`): child records that determine when a rule applies.
- Evaluation semantics:
  - `Visible` — first match wins (default: visible).
  - `Required` — first match wins (default: not required).
  - `NotAllowed` — deny overrides (any match with true blocks).

## Projects (high level)
- `DataversePolicyEngine.Core` — shared evaluator, repository, comparers (used by plugin and Custom API).
- `DataversePolicyEngine.Plugin` — PreOperation plugin enforcing Required and NotAllowed on Create/Update.
- `DataversePolicyEngine.CustomApi` — `EvaluatePolicies` Custom API used by JavaScript to get decisions in system context.
- `DataversePolicyEngine.Tests` — MSTest suite (FakeXrmEasy-based) covering plugin and evaluator behavior.

(See each project folder for project-level README and implementation details.)

## Getting Started
1. Open the solution in Visual Studio (the solution targets .NET Framework 4.6.2).
2. Restore NuGet packages.
3. Build the solution.
4. Run tests via Visual Studio Test Explorer (MSTest + FakeXrmEasy are used).

Notes:
- For plugin deployment to Dataverse, assemblies must be strong-named. The repository includes a key file (`dpe.snk`) and projects are configured to sign assemblies. Ensure signing is preserved if modifying project properties.

## Deployment & Registration
- Register the plugin assembly (`DataversePolicyEngine.Plugin`) with Dataverse.
- Register synchronous PreOperation steps for `Create` and `Update` on each governed entity.
  - Update step: include a PreImage named `PreImage` (all/relevant attributes).
- Use the Custom API `dpe_EvaluatePolicies` on forms to evaluate rules in system context.

See `docs/help.html` for a step-by-step builder guide and `docs/architecture/PolicyEngine-v1.md` for full architecture details.

## Troubleshooting
- If server enforcement appears ineffective, confirm:
  - The plugin steps are registered for the target table and the Update step includes the named PreImage.
  - Policy rules are `Active` and target the exact entity/attribute logical names.
- For client issues, ensure the form OnLoad handler is registered as `DPE.PolicyForm.onLoad` and the `config=` parameter is valid JSON (see `docs/help.html` example).

## Contributing
- Follow the existing architecture: add logic to `Core` for evaluator changes first so Custom API and Plugin stay in sync.
- Add/extend unit tests in `DataversePolicyEngine.Tests`.
- Preserve strong-naming in projects intended for Dataverse deployment.

## References
- docs/help.html — Builder guide (form wiring, plugin registration, example `config=`).
- docs/architecture/PolicyEngine-v1.md — architecture, data model, evaluation semantics.
- src/server/DataversePolicyEngine.Plugin/README.md — plugin specifics and registration guidance.
