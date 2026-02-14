# DataversePolicyEngine.Plugin

This project contains the Dataverse plugin(s) that enforce policies defined in the Dataverse Policy Engine tables.

The plugin enforces policy decisions at the database layer, independent of any client behavior.

---

## What This Plugin Enforces (v1)

Policies are defined in:

- `dpe_policyrule` (header)
- `dpe_policycondition` (child)

Policy types (v1):

- Visible (UI-only; not enforced server-side)
- Required (enforced)
- NotAllowed (enforced)

Enforcement rules (v1):

- **Required**
  - Create: if required and value is missing or null → block
  - Update: if required and value is being set to null → block
- **NotAllowed**
  - Create: if not allowed and attribute is being set → block
  - Update: if not allowed and value changes vs pre-image → block

Evaluation semantics:

- Visible → first match wins (default true)
- Required → first match wins (default false)
- NotAllowed → deny overrides (any matching true blocks)

---

## Architecture

The plugin does not implement policy logic directly.

It calls the shared engine:

- `DataversePolicyEngine.Core`
  - `PolicyEvaluator`
  - `PolicyRepository`
  - `ConditionComparer` / `DataverseValueComparer`

This keeps enforcement and evaluation consistent across:

- Plugin enforcement
- Custom API evaluation (planned)
- Client UX (planned JS)

---

## Registration Guidance (v1)

Register plugin steps on each governed table:

### Update
- Message: `Update`
- Stage: `PreOperation`
- Mode: `Synchronous`
- Pre Image:
  - Name: `PreImage`
  - Columns: include governed attributes (or all columns for early testing)

### Create
- Message: `Create`
- Stage: `PreOperation`
- Mode: `Synchronous`

Notes:

- The plugin evaluates only attributes that have at least one active rule for the entity.
- Policy tables use `statecode = 0` to indicate active rules (built-in active/inactive).

---

## Deployment Notes

- Assemblies must be **strong-named** for Dataverse plugin deployment.
- `DataversePolicyEngine.Core` is also signed to satisfy strong-name dependency rules.

---

## Files

- `PolicyEnforcementPlugin.cs`  
  Main enforcement plugin (Create/Update).

- `ValueEquality.cs`  
  Plugin-side equality helper for determining if an update actually changed a value
  (OptionSetValue, EntityReference, Money, etc.).

- `PluginBase.cs`  
  Common base used by the plugin(s) in this project.
