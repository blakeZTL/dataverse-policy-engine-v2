# DataversePolicyEngine.CustomApi

This project contains the Custom API implementation for the Dataverse Policy Engine.

The Custom API provides a system-context endpoint for evaluating policy rules without enforcing them.

It is intended for use by:

- Model-driven form JavaScript
- PCF controls
- External integrations
- Other server-side components

---

# Custom API: dpe_EvaluatePolicies

## Purpose

`dpe_EvaluatePolicies` evaluates policy decisions for a specific attribute using the shared Policy Engine.

It:

- Executes in **system context**
- Uses `DataversePolicyEngine.Core`
- Does NOT enforce changes
- Does NOT modify data
- Returns evaluation results only

This allows UI logic to be driven by policy rules without exposing policy tables to end users.

---

# Architecture

The Custom API calls:

DataversePolicyEngine.Core  
→ PolicyEvaluator  
→ PolicyRepository  
→ ConditionComparer  
→ DataverseValueComparer  

The enforcement plugin also calls the same engine.

This ensures identical behavior between:

- Database enforcement (plugin)
- UI evaluation (Custom API)
- Future integrations

---

# Request Parameters

Required:

- `EntityLogicalName` (string)
- `TargetAttributeLogicalName` (string)
- `TriggerAttributeLogicalName` (string)

Optional typed trigger value (provide one based on attribute type):

- `TriggerString` (string)
- `TriggerNumber` (decimal)
- `TriggerBoolean` (boolean)
- `TriggerOptionSetValue` (int)
- `TriggerLookupLogicalName` (string)
- `TriggerLookupId` (Guid)

If no trigger value is provided:
- The engine evaluates using null
- Useful for IsNull / IsNotNull operators

---

# Response Parameters

- `Visible` (bool)
- `Required` (bool)
- `NotAllowed` (bool)

Default behavior:

- Visible → true
- Required → false
- NotAllowed → false

---

# Evaluation Semantics (v1)

Policy types:

- Visible → first match wins
- Required → first match wins
- NotAllowed → deny overrides (any matching true blocks)

Conditions:

- AND-only
- Evaluated in sequence order
- All conditions must match for rule to apply

Rules:

- Evaluated in sequence order
- Only active rules (statecode = 0) are evaluated

---

# What This API Does NOT Do

- Does not retrieve existing records
- Does not evaluate PreImage
- Does not evaluate multiple attributes at once
- Does not enforce policies (plugin responsibility)
- Does not validate policy configuration
- Does not perform role-based filtering

These are either handled by the enforcement plugin or planned for future versions.

---

# Security Model

The Custom API runs in system context.

This allows:

- End users to execute the API
- Without requiring read access to:
  - dpe_policyrule
  - dpe_policycondition

This prevents cross-solution security coupling.

---

# Deployment Steps

1. Create an unbound Custom API in Dataverse:
   - Unique Name: `dpe_EvaluatePolicies`
   - Binding Type: None
   - Plugin Type: `EvaluatePoliciesCustomApi`

2. Add request and response parameters exactly as defined above.

3. Register the strongly-named assembly.

4. Assign appropriate execution privileges.

---

# Versioning

This is v1 of the Custom API.

Planned future enhancements:

- Multi-attribute batch evaluation
- Policy explanation tracing
- Role-aware policy filtering
- Cross-attribute conditions
- Caching layer
