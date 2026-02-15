# Dataverse Policy Engine — Client

Client-side code for Dataverse Policy Engine (DPE). Provides the form integration, helper functions to call the server `dpe_EvaluatePolicies` Custom API, and UI application helpers used by the model-driven form web resource.

## What lives here
- `src/policyForm.ts` — form wiring and OnLoad entrypoint used as the web resource handler (`DPE.PolicyForm.onLoad` in form registration).
- `src/policyClient.ts` — utility functions:
  - `readTriggerValue(formContext, triggerAttributeLogicalName)`
  - `evaluatePolicies(entityLogicalName, targetAttributeLogicalName, triggerAttributeLogicalName, triggerValue)`
  - `evaluatePoliciesBatch(...)`
  - `applyDecision(formContext, targetAttributeLogicalName, decision, applyNotAllowedAsDisabled)`
- `tests/` — unit tests (Vitest) that validate form wiring, batching, debounce and API payload typing.

## Quick usage
- Add the DPE web resource to your form libraries and register the OnLoad handler:
  - Function: `DPE.PolicyForm.onLoad`
  - Check: `Pass execution context as first parameter`
  - Parameter string: `config={...}` (see `docs/help.html` for a copy/paste example)
- Typical `config` keys:
  - `rules`: array of `{ target: "<attribute>", trigger: "<attribute>" }`
  - `debounceMs`: number (milliseconds)
  - `applyNotAllowedAsDisabled`: boolean
  - `entityLogicalName` (optional override)

See `docs/help.html` for a full example `config=` string and form wiring guidance.

## Building & testing (local)
- Install dependencies (project uses TypeScript + Vitest):
  - `npm install`
- Build/transpile as configured in the client project (see client `package.json` / build scripts).
- Run tests:
  - `npm test` or `npx vitest` (run from `src/client` folder; tests use `vitest` and DOM/XRM mocks).

## Integration notes
- The client calls the Custom API `dpe_EvaluatePolicies` (operation name `dpe_evaluatepolicies`) via `Xrm.WebApi.online.execute`. The client expects server responses shaped like:
  - For single: `{ Visible: boolean, Required: boolean, NotAllowed: boolean }`
  - For batch: `{ Results: [ { Target: "<attr>", Visible: bool, Required: bool, NotAllowed: bool }, ... ] }`
- `applyDecision` will:
  - set control visibility via `control.setVisible(...)`
  - set required level via `attribute.setRequiredLevel(...)`
  - optionally set control disabled when `NotAllowed` and `applyNotAllowedAsDisabled` is `true`
- The client intentionally asks the server to evaluate policies in system context (Custom API) so end users do not require read access to policy tables.

## Where to look next
- `docs/help.html` — builder guide and exact `config=` example.
- `src/client/tests` — test cases showing expected runtime behavior and API payload shapes.
- `src/server/DataversePolicyEngine.CustomApi` — server-side contract for `dpe_EvaluatePolicies`.
