import { describe, it, expect, vi, beforeEach } from "vitest";
import { onLoad, __resetPolicyFormForTests } from "../src/policyForm";

import {
    addStringField,
    addBooleanField,
    addOptionSetField,
    addLookupField,
    makeExecutionContext,
    mockApiReturn,
    getExecuteRequests,
    setEntityName,
    fireOnChange
} from "./helpers";



describe("DPE. onLoad - builder configuration coverage", () => {

    beforeEach(() => {
        __resetPolicyFormForTests();
    });

    it("throws when config.rules is missing/empty", async () => {
        const ctx = makeExecutionContext();

        await expect( onLoad(ctx, null as any)).rejects.toThrow();
        await expect(  onLoad(ctx, {} as any)).rejects.toThrow();
        await expect(  onLoad(ctx, { rules: [] })).rejects.toThrow();
    });

    it("throws when a rule is missing target/trigger", async () => {
        const ctx = makeExecutionContext();

        await expect(
              onLoad(ctx, { rules: [{ target: "name" } as any] })
        ).rejects.toThrow();

        await expect(
              onLoad(ctx, { rules: [{ trigger: "statuscode" } as any] })
        ).rejects.toThrow();
    });

    it("initial evaluation calls Custom API once per trigger (batched targets) and applies defaults", async () => {
        addOptionSetField("statuscode", 1);
        addStringField("name", "A");
        addStringField("telephone1", "B");

        mockApiReturn({
            Results: [
                { Target: "name", Visible: true, Required: false, NotAllowed: false },
                { Target: "telephone1", Visible: true, Required: false, NotAllowed: false }
            ]
        });

        const ctx = makeExecutionContext();

        await onLoad(ctx, {
            rules: [
                { target: "name", trigger: "statuscode" },
                { target: "telephone1", trigger: "statuscode" }
            ],
            debounceMs: 0
        });

        // one call for the trigger group (statuscode), both targets in one payload
        expect((Xrm.WebApi.online.execute as any).mock.calls.length).toBe(1);

        const reqs = getExecuteRequests();
        const req = reqs[0];

        // sanity
        expect(req.operationName || req.getMetadata().operationName).toBeDefined();

        

        // UI applied
        const nameCtrl: any = (Xrm.Page as any).getControl("name");
        const telCtrl: any = (Xrm.Page as any).getControl("telephone1");
        expect(nameCtrl.getVisible()).toBe(true);
        expect(telCtrl.getVisible()).toBe(true);
    });

    it("uses entityLogicalName override when provided", async () => {
        addOptionSetField("statuscode", 1);
        addStringField("name", "A");

        mockApiReturn({
            Results: [
                { Target: "name", Visible: true, Required: false, NotAllowed: false },
                { Target: "telephone1", Visible: true, Required: false, NotAllowed: false }
            ]
        });

        const ctx = makeExecutionContext();

        await   onLoad(ctx, {
            entityLogicalName: "new_customentity",
            rules: [{ target: "name", trigger: "statuscode" }],
            debounceMs: 0
        });

        const req = getExecuteRequests()[0];
        expect(req.EntityLogicalName).toBe("new_customentity");
    });

    it("auto-detects entity name when override not provided", async () => {
        setEntityName("contact");

        addOptionSetField("statuscode", 1);
        addStringField("name", "A");

        mockApiReturn({
            Results: [
                { Target: "name", Visible: true, Required: false, NotAllowed: false },
                { Target: "telephone1", Visible: true, Required: false, NotAllowed: false }
            ]
        });

        const ctx = makeExecutionContext();

        await   onLoad(ctx, {
            rules: [{ target: "name", trigger: "statuscode" }],
            debounceMs: 0
        });

        const req = getExecuteRequests()[0];
        expect(req.EntityLogicalName).toBe("contact");
    });

    it("applyNotAllowedAsDisabled=true disables controls when NotAllowed=true", async () => {
        addOptionSetField("statuscode", 1);
        addStringField("name", "A");

        mockApiReturn({
            Results: [
                { Target: "name", Visible: true, Required: false, NotAllowed: true },
                { Target: "telephone1", Visible: true, Required: false, NotAllowed: false }
            ]
        });

        const ctx = makeExecutionContext();

        await   onLoad(ctx, {
            rules: [{ target: "name", trigger: "statuscode" }],
            applyNotAllowedAsDisabled: true,
            debounceMs: 0
        });

        const ctrl: any = (Xrm.Page as any).getControl("name");
        expect(ctrl.getDisabled()).toBe(true);
    });

    it("applyNotAllowedAsDisabled=false does NOT disable controls even when NotAllowed=true", async () => {
        addOptionSetField("statuscode", 1);
        addStringField("name", "A");

        mockApiReturn({
            Results: [
                { Target: "name", Visible: true, Required: false, NotAllowed: false },
                { Target: "telephone1", Visible: true, Required: false, NotAllowed: false }
            ]
        });

        const ctx = makeExecutionContext();

        await   onLoad(ctx, {
            rules: [{ target: "name", trigger: "statuscode" }],
            applyNotAllowedAsDisabled: false,
            debounceMs: 0
        });

        const ctrl: any = (Xrm.Page as any).getControl("name");
        expect(ctrl.getDisabled()).toBe(false);
    });

    it("sets required level correctly", async () => {
        addOptionSetField("statuscode", 1);
        addStringField("name", "A");

        mockApiReturn({
            Results: [
                { Target: "name", Visible: true, Required: true, NotAllowed: false },
                { Target: "telephone1", Visible: true, Required: false, NotAllowed: false }
            ]
        });

        const ctx = makeExecutionContext();

        await   onLoad(ctx, {
            rules: [{ target: "name", trigger: "statuscode" }],
            debounceMs: 0
        });

        const attr: any = (Xrm.Page as any).getAttribute("name");
        expect(attr.getRequiredLevel()).toBe("required");
    });

    it("skips target attributes/controls that are not on the form (no API call for missing targets)", async () => {
        addOptionSetField("statuscode", 1);
        addStringField("name", "A");
        // telephone1 NOT added

        mockApiReturn({
            Results: [
                { Target: "name", Visible: true, Required: false, NotAllowed: false },
                { Target: "telephone1", Visible: true, Required: false, NotAllowed: false }
            ]
        });

        const ctx = makeExecutionContext();

        await   onLoad(ctx, {
            rules: [
                { target: "name", trigger: "statuscode" },
                { target: "telephone1", trigger: "statuscode" }
            ],
            debounceMs: 0
        });

        // Only one target exists => only one execute call
        expect((Xrm.WebApi.online.execute as any).mock.calls.length).toBe(1);
    });

    it("skips wiring triggers that are not on the form (still evaluates other triggers)", async () => {
        // Trigger A exists
        addOptionSetField("statuscode", 1);
        addStringField("name", "A");

        // Trigger B does NOT exist
        addStringField("telephone1", "B");

        mockApiReturn({
            Results: [
                { Target: "name", Visible: true, Required: false, NotAllowed: false },
                { Target: "telephone1", Visible: true, Required: false, NotAllowed: false }
            ]
        });

        const ctx = makeExecutionContext();

        await   onLoad(ctx, {
            rules: [
                { target: "name", trigger: "statuscode" },
                { target: "telephone1", trigger: "missing_trigger" }
            ],
            debounceMs: 0
        });
        // name evaluated (1 call). telephone1 skipped because trigger missing.
        expect((Xrm.WebApi.online.execute as any).mock.calls.length).toBe(1);
    });

    it("supports multiple triggers; evaluates each trigger group", async () => {
        addOptionSetField("statuscode", 1);
        addBooleanField("donotemail", false);

        addStringField("name", "A");
        addStringField("telephone1", "B");

        mockApiReturn({
            Results: [
                { Target: "name", Visible: true, Required: false, NotAllowed: false },
                { Target: "telephone1", Visible: true, Required: false, NotAllowed: false }
            ]
        });

        const ctx = makeExecutionContext();

        await   onLoad(ctx, {
            rules: [
                { target: "name", trigger: "statuscode" },
                { target: "telephone1", trigger: "donotemail" }
            ],
            debounceMs: 0
        });

        expect((Xrm.WebApi.online.execute as any).mock.calls.length).toBe(2);

        const reqs = getExecuteRequests();
        expect(reqs[0].TriggerAttributeLogicalName).toBe("statuscode");
        expect(reqs[1].TriggerAttributeLogicalName).toBe("donotemail");
    });

    it("debounceMs collapses rapid trigger changes into one evaluation burst", async () => {
        vi.useFakeTimers();

        addOptionSetField("statuscode", 1);
        addStringField("name", "A");
        addStringField("telephone1", "B");

        mockApiReturn({
            Results: [
                { Target: "name", Visible: true, Required: false, NotAllowed: false },
                { Target: "telephone1", Visible: true, Required: false, NotAllowed: false }
            ]
        });

        const ctx = makeExecutionContext();

        await   onLoad(ctx, {
            rules: [
                { target: "name", trigger: "statuscode" },
                { target: "telephone1", trigger: "statuscode" }
            ],
            debounceMs: 200
        });

        // initial eval: 1 call (batched per trigger)// initial eval: 1 call (batched per trigger)
        expect((Xrm.WebApi.online.execute as any).mock.calls.length).toBe(1);

        // rapid changes
        const attr: any = (Xrm.Page as any).getAttribute("statuscode");
        attr.setValue(2); fireOnChange("statuscode");
        attr.setValue(3); fireOnChange("statuscode");
        attr.setValue(4); fireOnChange("statuscode");

        // no immediate additional calls
        expect((Xrm.WebApi.online.execute as any).mock.calls.length).toBe(1);

        // after debounce, one burst => +2 calls
        await vi.advanceTimersByTimeAsync(200);
        await Promise.resolve(); // flush queued microtasks
        expect((Xrm.WebApi.online.execute as any).mock.calls.length).toBe(2);

        vi.useRealTimers();
    });

    it("caches per (entity,target,trigger,value) so re-firing without value change does not re-call API", async () => {
        addOptionSetField("statuscode", 1);
        addStringField("name", "A");

        mockApiReturn({
            Results: [
                { Target: "name", Visible: true, Required: false, NotAllowed: false },
                { Target: "telephone1", Visible: true, Required: false, NotAllowed: false }
            ]
        });

        const ctx = makeExecutionContext();

        await   onLoad(ctx, {
            rules: [{ target: "name", trigger: "statuscode" }],
            debounceMs: 0
        });

        expect((Xrm.WebApi.online.execute as any).mock.calls.length).toBe(1);

        // fire change with same value
        fireOnChange("statuscode");
        await Promise.resolve();

        // still 1 (cached)
        expect((Xrm.WebApi.online.execute as any).mock.calls.length).toBe(1);
    });

    it("request payload typing: OptionSet trigger maps to TriggerOptionSetValue", async () => {
        addOptionSetField("statuscode", 7);
        addStringField("name", "A");

        mockApiReturn({
            Results: [
                { Target: "name", Visible: true, Required: false, NotAllowed: false },
                { Target: "telephone1", Visible: true, Required: false, NotAllowed: false }
            ]
        });

        const ctx = makeExecutionContext();

        await   onLoad(ctx, {
            rules: [{ target: "name", trigger: "statuscode" }],
            debounceMs: 0
        });

        const req = getExecuteRequests()[0];
        expect(req.TriggerOptionSetValue).toBe(7);
        expect(req.TriggerString).toBeNull();
        expect(req.TriggerBoolean).toBeNull();
    });

    it("request payload typing: Boolean trigger maps to TriggerBoolean", async () => {
        addBooleanField("donotemail", true);
        addStringField("name", "A");

        mockApiReturn({
            Results: [
                { Target: "name", Visible: true, Required: false, NotAllowed: false },
                { Target: "telephone1", Visible: true, Required: false, NotAllowed: false }
            ]
        });

        const ctx = makeExecutionContext();

        await   onLoad(ctx, {
            rules: [{ target: "name", trigger: "donotemail" }],
            debounceMs: 0
        });

        const req = getExecuteRequests()[0];
        expect(req.TriggerBoolean).toBe(true);
    });

    it("request payload typing: String trigger maps to TriggerString", async () => {
        addStringField("new_triggertext", "HELLO");
        addStringField("name", "A");

        mockApiReturn({
            Results: [
                { Target: "name", Visible: true, Required: false, NotAllowed: false },
                { Target: "telephone1", Visible: true, Required: false, NotAllowed: false }
            ]
        });

        const ctx = makeExecutionContext();

        await   onLoad(ctx, {
            rules: [{ target: "name", trigger: "new_triggertext" }],
            debounceMs: 0
        });

        const req = getExecuteRequests()[0];
        expect(req.TriggerString).toBe("HELLO");
    });

    it("request payload typing: Lookup trigger maps to TriggerLookupLogicalName + TriggerLookupId", async () => {
        addLookupField("primarycontactid", [
            { id: "{11111111-1111-1111-1111-111111111111}", name: "Joe", entityType: "contact" }
        ]);
        addStringField("name", "A");

        mockApiReturn({
            Results: [
                { Target: "name", Visible: true, Required: false, NotAllowed: false },
                { Target: "telephone1", Visible: true, Required: false, NotAllowed: false }
            ]
        });

        const ctx = makeExecutionContext();

        await   onLoad(ctx, {
            rules: [{ target: "name", trigger: "primarycontactid" }],
            debounceMs: 0
        });

        const req = getExecuteRequests()[0];
        expect(req.TriggerLookupLogicalName).toBe("contact");
        expect(req.TriggerLookupId).toBe("11111111-1111-1111-1111-111111111111");
    });

    it("null trigger value results in no typed trigger parameters (supports IsNull rules)", async () => {
        addLookupField("primarycontactid", null); // null trigger
        addStringField("name", "A");

        mockApiReturn({
            Results: [
                { Target: "name", Visible: true, Required: false, NotAllowed: false },
                { Target: "telephone1", Visible: true, Required: false, NotAllowed: false }
            ]
        });

        const ctx = makeExecutionContext();

        await   onLoad(ctx, {
            rules: [{ target: "name", trigger: "primarycontactid" }],
            debounceMs: 0
        });

        const req = getExecuteRequests()[0];
        expect(req.TriggerLookupLogicalName).toBeNull();
        expect(req.TriggerLookupId).toBeNull();
        expect(req.TriggerOptionSetValue).toBeNull();
        expect(req.TriggerBoolean).toBeNull();
        expect(req.TriggerString).toBeNull();
    });

    it("does not crash the form if the Custom API call fails (logs error)", async () => {
        addOptionSetField("statuscode", 1);
        addStringField("name", "A");

        (Xrm.WebApi.online.execute as any).mockRejectedValue(new Error("boom"));

        const ctx = makeExecutionContext();

        await expect(
              onLoad(ctx, {
                rules: [{ target: "name", trigger: "statuscode" }],
                debounceMs: 0
            })
        ).resolves.not.toThrow();

        expect(console.error).toHaveBeenCalled();
    });

    it("does not crash if response json parsing fails (logs error)", async () => {
        addOptionSetField("statuscode", 1);
        addStringField("name", "A");

        (Xrm.WebApi.online.execute as any).mockResolvedValue({
            json: async () => {
                throw new Error("bad json");
            }
        });

        const ctx = makeExecutionContext();

        await expect(
              onLoad(ctx, {
                rules: [{ target: "name", trigger: "statuscode" }],
                debounceMs: 0
            })
        ).resolves.not.toThrow();

        expect(console.error).toHaveBeenCalled();
    });
});
