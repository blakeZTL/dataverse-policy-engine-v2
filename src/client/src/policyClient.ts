import type { PolicyDecision, TriggerValue } from "./types";

const OPERATION_NAME = "dpe_EvaluatePolicies";

function stripBraces(guid: string): string {
    return guid.replace("{", "").replace("}", "");
}

export function readTriggerValue(formContext: Xrm.FormContext, triggerAttributeLogicalName: string): TriggerValue {
    const attr = formContext.getAttribute(triggerAttributeLogicalName as any);
    if (!attr) return null;

    const v = attr.getValue() as any;
    if (v === null || v === undefined) return null;

    // Lookup: [{ id, name, entityType }]
    if (Array.isArray(v) && v.length > 0 && v[0]?.id && v[0]?.entityType) {
        return {
            kind: "lookup",
            lookupLogicalName: v[0].entityType,
            lookupId: stripBraces(v[0].id)
        };
    }

    // OptionSet: number
    if (typeof v === "number") {
        return { kind: "optionset", optionSetValue: v };
    }

    if (typeof v === "boolean") return { kind: "boolean", value: v };
    if (typeof v === "string") return { kind: "string", value: v };

    // Fallback: coerce to string
    return { kind: "string", value: String(v) };
}

export type BatchedPolicyResult = Record<string, PolicyDecision>;

export async function evaluatePoliciesBatch(
    entityLogicalName: string,
    targetAttributeLogicalNames: string[],
    triggerAttributeLogicalName: string,
    triggerValue: TriggerValue
): Promise<BatchedPolicyResult> {
    const request: any = {
        EntityLogicalName: entityLogicalName,
        TargetAttributeLogicalNames: targetAttributeLogicalNames,
        TriggerAttributeLogicalName: triggerAttributeLogicalName,

        TriggerString: null,
        TriggerNumber: null,
        TriggerBoolean: null,
        TriggerOptionSetValue: null,
        TriggerLookupLogicalName: null,
        TriggerLookupId: null,

        getMetadata: function () {
            return {
                boundParameter: null,
                parameterTypes: {
                    EntityLogicalName: { typeName: "Edm.String", structuralProperty: 1 },

                    // string array
                    TargetAttributeLogicalNames: { typeName: "Collection(Edm.String)", structuralProperty: 4 },

                    TriggerAttributeLogicalName: { typeName: "Edm.String", structuralProperty: 1 },
                    TriggerString: { typeName: "Edm.String", structuralProperty: 1 },
                    TriggerNumber: { typeName: "Edm.Decimal", structuralProperty: 1 },
                    TriggerBoolean: { typeName: "Edm.Boolean", structuralProperty: 1 },
                    TriggerOptionSetValue: { typeName: "Edm.Int32", structuralProperty: 1 },
                    TriggerLookupLogicalName: { typeName: "Edm.String", structuralProperty: 1 },
                    TriggerLookupId: { typeName: "Edm.Guid", structuralProperty: 1 }
                },
                operationName: OPERATION_NAME,
                operationType: 0
            };
        }
    };

    if (triggerValue) {
        switch (triggerValue.kind) {
            case "lookup":
                request.TriggerLookupLogicalName = triggerValue.lookupLogicalName;
                request.TriggerLookupId = triggerValue.lookupId;
                break;
            case "optionset":
                request.TriggerOptionSetValue = triggerValue.optionSetValue;
                break;
            case "boolean":
                request.TriggerBoolean = triggerValue.value;
                break;
            case "number":
                request.TriggerNumber = triggerValue.value;
                break;
            case "string":
                request.TriggerString = triggerValue.value;
                break;
        }
    }

    const resp = await Xrm.WebApi.online.execute(request);
    const json = (await resp.json()) as any;

    let parsed: any = json;
    if (typeof json.ResultsJson === "string") {
        try {
            parsed = JSON.parse(json.ResultsJson);
        } catch {
            // leave parsed as json; your "items" will likely be []
        }
    }

    // Expecting server to return an array of results
    // Shape: { Results: [{ Target: "name", Visible: true, Required: false, NotAllowed: false }, ...] }
    const results: BatchedPolicyResult = {};

    const items: any[] = Array.isArray(parsed.Results) ? parsed.Results : [];
    for (const r of items) {
        const target = r.Target as string;
        if (!target) continue;

        results[target] = {
            visible: r.Visible === true,
            required: r.Required === true,
            notAllowed: r.NotAllowed === true
        };
    }

    // Default any missing targets
    for (const t of targetAttributeLogicalNames) {
        if (!results[t]) {
            results[t] = { visible: true, required: false, notAllowed: false };
        }
    }

    return results;
}


export async function evaluatePolicies(
    entityLogicalName: string,
    targetAttributeLogicalName: string,
    triggerAttributeLogicalName: string,
    triggerValue: TriggerValue
): Promise<PolicyDecision> {
    const request: any = {
        EntityLogicalName: entityLogicalName,
        TargetAttributeLogicalName: targetAttributeLogicalName,
        TriggerAttributeLogicalName: triggerAttributeLogicalName,

        TriggerString: null,
        TriggerNumber: null,
        TriggerBoolean: null,
        TriggerOptionSetValue: null,
        TriggerLookupLogicalName: null,
        TriggerLookupId: null,

        getMetadata: function () {
            return {
                boundParameter: null,
                parameterTypes: {
                    EntityLogicalName: { typeName: "Edm.String", structuralProperty: 1 },
                    TargetAttributeLogicalName: { typeName: "Edm.String", structuralProperty: 1 },
                    TriggerAttributeLogicalName: { typeName: "Edm.String", structuralProperty: 1 },
                    TriggerString: { typeName: "Edm.String", structuralProperty: 1 },
                    TriggerNumber: { typeName: "Edm.Decimal", structuralProperty: 1 },
                    TriggerBoolean: { typeName: "Edm.Boolean", structuralProperty: 1 },
                    TriggerOptionSetValue: { typeName: "Edm.Int32", structuralProperty: 1 },
                    TriggerLookupLogicalName: { typeName: "Edm.String", structuralProperty: 1 },
                    TriggerLookupId: { typeName: "Edm.Guid", structuralProperty: 1 }
                },
                operationName: OPERATION_NAME,
                operationType: 0
            };
        }
    };

    if (triggerValue) {
        switch (triggerValue.kind) {
            case "lookup":
                request.TriggerLookupLogicalName = triggerValue.lookupLogicalName;
                request.TriggerLookupId = triggerValue.lookupId;
                break;
            case "optionset":
                request.TriggerOptionSetValue = triggerValue.optionSetValue;
                break;
            case "boolean":
                request.TriggerBoolean = triggerValue.value;
                break;
            case "number":
                request.TriggerNumber = triggerValue.value;
                break;
            case "string":
                request.TriggerString = triggerValue.value;
                break;
        }
    }

    const resp = await Xrm.WebApi.online.execute(request);
    const json = (await resp.json()) as any;

    return {
        visible: json.Visible === true,
        required: json.Required === true,
        notAllowed: json.NotAllowed === true
    };
}

export function applyDecision(
    formContext: Xrm.FormContext,
    targetAttributeLogicalName: string,
    decision: PolicyDecision,
    applyNotAllowedAsDisabled: boolean
): void {
    const ctrl = formContext.getControl(targetAttributeLogicalName as any) as Xrm.Controls.StandardControl;
    const attr = formContext.getAttribute(targetAttributeLogicalName as any);

    if (ctrl) {
        ctrl.setVisible(!!decision.visible);
        if (applyNotAllowedAsDisabled) ctrl.setDisabled(!!decision.notAllowed);
    }

    if (attr) {
        attr.setRequiredLevel(decision.required ? "required" : "none");
    }
}
