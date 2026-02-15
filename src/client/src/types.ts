export type TriggerValue =
    | null
    | { kind: "string"; value: string }
    | { kind: "number"; value: number }
    | { kind: "boolean"; value: boolean }
    | { kind: "optionset"; optionSetValue: number }
    | { kind: "lookup"; lookupLogicalName: string; lookupId: string };

export type PolicyDecision = {
    visible: boolean;
    required: boolean;
    notAllowed: boolean;
};

export type PolicyRuleMapping = {
    target: string;
    trigger: string;
};

export type PolicyFormConfig = {
    entityLogicalName?: string;
    rules: PolicyRuleMapping[];
    debounceMs?: number;
    applyNotAllowedAsDisabled?: boolean;
    trace?: boolean;   // logs payloads, responses, cache hits, skips
    strict?: boolean;  // throw on misconfig (missing trigger/target/control)
};
