# DataversePolicyEngine Plugin Tests

This test project validates server-side enforcement behavior for the Policy Engine plugin(s).

The tests are written with:

- MSTest
- FakeXrmEasy (Dataverse runtime simulation)

---

## What These Tests Cover

### Policy Enforcement Plugin

Validates Create/Update enforcement behaviors:

- **NotAllowed**
  - Update: blocks when the attribute value changes and a matching NotAllowed rule applies
  - Update: does not block when value is unchanged
  - Create: blocks when the attribute is set and a matching NotAllowed rule applies
  - Create: does not block when the NotAllowed rule does not match

- **Required**
  - Update: blocks when a matching Required rule exists and the user attempts to null the attribute
  - Create: blocks when a matching Required rule exists and the attribute is missing or null
  - Allows when the attribute is supplied

---

## What These Tests Do NOT Cover

These are intentionally excluded from this project:

- Dataverse security context behavior (system vs user)
- Plugin step registration correctness in an actual environment
- Real metadata-driven validation of policy table configuration
- Client-side JavaScript UX behavior
- Custom API wiring (planned)

Those require integration testing in a real Dataverse environment.

---

## How Tests Are Structured

- Tests inherit from a shared `FakeXRMTestBase` that provides `_context`
- Policy tables (`dpe_policyrule`, `dpe_policycondition`) are initialized into the fake context
- Plugin execution contexts are created via `_context.GetDefaultPluginContext()`
- Plugin execution is run via `_context.ExecutePluginWith<TPlugin>(...)`

---

## Notes

- Assemblies are strong-named to match real Dataverse deployment requirements.
- If you see a FileLoadException related to strong naming, verify that all referenced assemblies are signed.
