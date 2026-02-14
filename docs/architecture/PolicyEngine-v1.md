# Dataverse Policy Engine – Architecture (v1)

Version: 1.0  
Last Updated: 2026-02-13  
Status: Stable (v1)

---

# Purpose

Provide dynamic, configurable attribute-level policies in Dataverse that control:

- Visibility
- Required state
- Edit restrictions (Not Allowed)

Policies are enforced:
- Client-side (JavaScript) for user experience
- Server-side (Plugin) for authoritative enforcement

---

# Core Design Decisions

## Policy Types

PolicyType (single select choice):

- Visible
- Required
- NotAllowed

### Defaults

- Visible = true
- Required = false
- NotAllowed = false (Allowed by default)

### Evaluation Strategy

- Visible → First matching rule wins
- Required → First matching rule wins
- NotAllowed → Deny overrides (any matching rule blocks)

---

# Data Model

## 1️⃣ Policy Rule (Header)

Represents:
- What attribute is being controlled
- What type of policy applies

### Columns

- TargetEntityLogicalName (text)
- TargetAttributeLogicalName (text)
- PolicyType (choice: Visible / Required / NotAllowed)
- Result (boolean)
- Sequence (whole number)
- IsActive (yes/no)

### Notes

- Lower Sequence number = higher priority
- Only one PolicyType per rule
- Deterministic evaluation order

---

## 2️⃣ Policy Condition (Child of Policy Rule)

Represents:
- When the rule applies

### Columns

- PolicyRule (lookup)
- Sequence (whole number)
- Operator (choice):
  - Equals
  - NotEquals
  - IsNull
  - IsNotNull

- ValueType (choice):
  - String
  - Number
  - Boolean
  - OptionSet
  - Lookup

### Typed Value Columns

- ValueString (text)
- ValueNumber (decimal)
- ValueBoolean (yes/no)
- ValueOptionSetValue (whole number)
- ValueLookupLogicalName (text)
- ValueLookupId (uniqueidentifier)

### Condition Logic

- All conditions under a rule are AND
- Evaluated in Sequence order
- Rule matches only if all conditions evaluate true

---

# Runtime Evaluation Model

## Evaluation Steps

1. Load Policy Rules where:
   - TargetEntityLogicalName matches entity
   - TargetAttributeLogicalName matches attribute
   - IsActive = true

2. Sort by PolicyRule.Sequence ascending

3. For each rule:
   - Evaluate all Policy Conditions (AND)
   - If match → apply rule logic

---

## Visible

- First matching rule wins
- Apply rule.Result
- Stop evaluating
- If none match → Visible = true

---

## Required

- First matching rule wins
- Apply rule.Result
- Stop evaluating
- If none match → Required = false

---

## NotAllowed

- Deny overrides
- If any matching rule has Result = true → NotAllowed = true
- Stop immediately
- If none match → NotAllowed = false

---

# Enforcement Behavior

## Client (Model-Driven App JavaScript)

- Applies Visible and Required dynamically
- Disables controls if NotAllowed = true
- Re-evaluates on trigger attribute changes

---

## Server (Plugin – Create/Update)

### NotAllowed Enforcement

On Update:
- If attribute present in Target
- Compare Target value vs PreImage value
- If different → throw InvalidPluginExecutionException

On Create:
- If attribute set and rule matches → throw exception

### Required Enforcement

On Create:
- If Required = true and attribute null → throw

On Update:
- If attribute being set to null and Required = true → throw

---

# Lookup Comparison Strategy

Lookup values are stored as:

- ValueLookupLogicalName (text)
- ValueLookupId (guid)

This avoids creating polymorphic lookup relationships and allows comparison against any table.

Comparison logic:
- EntityReference.LogicalName == ValueLookupLogicalName
- EntityReference.Id == ValueLookupId

---

# Limitations (v1)

- Operators limited to Equals, NotEquals, IsNull, IsNotNull
- AND-only conditions
- One PolicyType per rule
- No OR groups
- No cross-attribute comparison
- No role/user-based context filtering

This version is intentionally minimal and deterministic.

---

# Architecture Diagram

```mermaid
flowchart TD
    A[Dataverse Operation] --> B[Plugin]
    B --> C[Load Policy Rules]
    C --> D[Evaluate Conditions]
    D --> E{PolicyType}
    E -->|Visible| F[Apply First Match]
    E -->|Required| G[Apply First Match]
    E -->|NotAllowed| H[Deny Overrides]

---

# Design Philosophy

- Deterministic rule evaluation

- Minimal schema complexity

- Strong server-side enforcement

- Configurable without code changes

- Avoid relationship explosion

- Secure by default

---