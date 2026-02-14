# Dataverse Policy Engine – Roadmap

---

# v2.1 – Expanded Operators

Add support for:

- GreaterThan
- GreaterOrEqual
- LessThan
- LessOrEqual
- Contains (string)
- StartsWith (string)
- Changed (Update-only; requires PreImage)

---

# v2.2 – Cross-Attribute Comparison

Enhance Policy Condition with:

- CompareTo (Constant | AnotherAttribute)
- CompareAttributeLogicalName (text)

Allows rules such as:

"If EndDate < StartDate → NotAllowed"

---

# v2.3 – OR Condition Groups

Add:

- ConditionGroup (whole number)

Evaluation logic:

- OR across groups
- AND within each group

Example:

(Group 1: A AND B)  
OR  
(Group 2: C AND D)

---

# v2.4 – Multiple Target Attributes Per Rule

Introduce:

## Policy Target (Child of Policy Rule)

Columns:
- PolicyRule (lookup)
- TargetAttributeLogicalName (text)
- Sequence

Allows one condition set to control multiple attributes without duplication.

---

# v2.5 – Server-Based Evaluation API

Add Custom API:

EvaluatePolicies(
    entityLogicalName,
    recordId,
    changedAttributes
)

Returns effective:
- Visible
- Required
- NotAllowed decisions

Benefits:
- JavaScript no longer needs direct read access to policy tables
- Centralized logic
- Improved security
- Consistent evaluation between client and server

---

# v2.6 – Diagnostics & Explain Mode

Enhance evaluation engine to optionally return:

- Matched Rule IDs
- Failed Condition IDs
- Evaluation trace

Useful for:
- Admin troubleshooting
- Debug UI
- Logging

---

# v2.7 – Performance & Caching

- Cache active rules per entity
- Cache rule sets by attribute
- Invalidate cache when policy records change
- Reduce plugin query overhead

---

# v2.8 – Context-Based Policies

Add optional rule filters:

- AppliesOn (Create | Update | Both)
- Execution Stage (PreOperation | PostOperation)
- User Role filter
- Business Unit filter
- Team filter

---

# Long-Term Vision

- Full metadata-driven rule engine
- Policy authoring UI (Custom Page or PCF)
- Rule validation on save
- Visual policy dependency graph
- Enterprise-grade extensibility framework

---

# Guiding Principles for Future Versions

- Keep v1 deterministic
- Add complexity only when needed
- Preserve backward compatibility
- Prefer configuration over schema expansion
- Keep enforcement centralized in plugin layer