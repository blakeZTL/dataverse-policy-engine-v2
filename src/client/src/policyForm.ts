import type { PolicyFormConfig, PolicyDecision } from "./types";
import { applyDecision,  readTriggerValue, evaluatePoliciesBatch } from "./policyClient";
import { validatePolicyFormConfig } from "./configValidator";


function groupByTrigger(rules: PolicyFormConfig["rules"]): Record<string, string[]> {
    const map: Record<string, string[]> = {};
    for (const r of rules) {
        if (!map[r.trigger]) map[r.trigger] = [];
        map[r.trigger].push(r.target);
    }
    return map;
}

export function makeDebouncer(ms: number) {
    let handle: ReturnType<typeof setTimeout> | undefined;

    return (fn: () => void | Promise<void>) => {
        if (handle) {
            globalThis.clearTimeout(handle);
            handle = undefined;
        }

        handle = globalThis.setTimeout(() => {
            handle = undefined;
            void fn();
        }, ms);
    };
}


function getEntityLogicalName(formContext: Xrm.FormContext, config: PolicyFormConfig): string {
    return config.entityLogicalName ?? formContext.data.entity.getEntityName();
}

function serializeTriggerValue(triggerValue: any): string {
    if (!triggerValue) return "null";

    switch (triggerValue.kind) {
        case "lookup":
            return `lookup:${triggerValue.lookupLogicalName}:${triggerValue.lookupId}`;
        case "optionset":
            return `optionset:${triggerValue.optionSetValue}`;
        case "boolean":
            return `boolean:${String(triggerValue.value)}`;
        case "number":
            return `number:${String(triggerValue.value)}`;
        case "string":
            return `string:${String(triggerValue.value)}`;
        default:
            return `string:${String(triggerValue)}`;
    }
}

function makeCacheKey(entity: string, target: string, trigger: string, triggerValue: any): string {
    const v = serializeTriggerValue(triggerValue);
    return `${entity}|${target}|${trigger}|${v}`;
}

async function evaluateAndApplyForTrigger(
    formContext: Xrm.FormContext,
    entity: string,
    triggerAttr: string,
    targets: string[],
    config: PolicyFormConfig,
    cache: Record<string, any>
) {
    const triggerValue = readTriggerValue(formContext, triggerAttr);
    const applyNotAllowedAsDisabled = config.applyNotAllowedAsDisabled !== false;

    // only keep targets that exist on the form (attribute OR control)
    const existingTargets: string[] = [];
    for (const targetAttr of targets) {
        const ctrl = formContext.getControl(targetAttr as any);
        const attr = formContext.getAttribute(targetAttr as any);
        if (!ctrl && !attr) continue;
        existingTargets.push(targetAttr);
    }

    if (existingTargets.length === 0) return;

    // one cache key per trigger group (batched)
    // Sorting makes it stable regardless of rule order
    //const batchKey = `BATCH|${entity}|${triggerAttr}|${serializeTriggerValue(triggerValue)}|${existingTargets
    //    .slice()
    //    .sort()
    //    .join(",")}`;

    const batchTargetId = `__BATCH__:${existingTargets.slice().sort().join(",")}`;
    const batchKey = makeCacheKey(entity, batchTargetId, triggerAttr, triggerValue);

    let decisionsByTarget = cache[batchKey] as Record<string, PolicyDecision> | undefined;
    if (!decisionsByTarget) {
        decisionsByTarget = await evaluatePoliciesBatch(entity, existingTargets, triggerAttr, triggerValue);
        cache[batchKey] = decisionsByTarget;
    }

    for (const targetAttr of existingTargets) {
        const decision = decisionsByTarget[targetAttr] ?? { visible: true, required: false, notAllowed: false };
        applyDecision(formContext, targetAttr, decision, applyNotAllowedAsDisabled);
    }
}


let wiredForms = new WeakSet<any>();
let cacheByForm = new WeakMap<any, Record<string, any>>();

function traceEnabled(config: PolicyFormConfig): boolean {
    return config.trace === true;
}

function traceLog(config: PolicyFormConfig, ...args: any[]): void {
    if (!traceEnabled(config)) return;
    // eslint-disable-next-line no-console
    console.debug("[DPE.Trace]", ...args);
}

function assertOrSkip(config: PolicyFormConfig, condition: any, message: string): boolean {
    if (condition) return true;
    if (config.strict) throw new Error(message);
    return false;
}

async function evaluateAll(
    formContext: Xrm.FormContext,
    config: PolicyFormConfig,
    cache: Record<string, any>
): Promise<void> {
    const triggerToTargets = groupByTrigger(config.rules);
    const entity = getEntityLogicalName(formContext, config);

    for (const triggerAttr of Object.keys(triggerToTargets)) {
        const triggerAttribute = formContext.getAttribute(triggerAttr as any);

        // Strict: trigger must exist; Non-strict: skip evaluation for missing trigger
        if (!assertOrSkip(config, triggerAttribute, `DPE.PolicyForm strict mode: trigger '${triggerAttr}' is not on the form.`)) {
            traceLog(config, `Skipping trigger '${triggerAttr}' (not on form)`);
            continue;
        }

        traceLog(config, `Evaluating trigger '${triggerAttr}' for ${triggerToTargets[triggerAttr].length ?? "?"} targets`);

        await evaluateAndApplyForTrigger(
            formContext,
            entity,
            triggerAttr,
            triggerToTargets[triggerAttr],
            config,
            cache
        );
    }
}

export async function onLoad(
    executionContext: Xrm.Events.EventContext,
    config: PolicyFormConfig
): Promise<void> {
    const validation = validatePolicyFormConfig(config);
    if (!validation.ok) throw new Error(validation.message);

    const formContext = executionContext.getFormContext();
    const debounceMs = typeof config.debounceMs === "number" ? config.debounceMs : 150;

    // persistent cache per form instance (helps perf + supports reEvaluate)
    let cache = cacheByForm.get(formContext);
    if (!cache) {
        cache = {};
        cacheByForm.set(formContext, cache);
    }

    const debouncer = makeDebouncer(debounceMs);

    // initial eval (respects strict + missing trigger behavior)
    try {
        await evaluateAll(formContext, config, cache);
    } catch (e) {
        // don’t break form
        // eslint-disable-next-line no-console
        console.error("DPE.PolicyForm initial evaluation failed:", e);
    }

    // duplicate wiring guard
    if (wiredForms.has(formContext)) {        
        traceLog(config, "Form already wired; skipping addOnChange wiring");
        return;
    }
    wiredForms.add(formContext);

    // wire triggers (only those present; strict throws)
    const triggerToTargets = groupByTrigger(config.rules);

    for (const triggerAttr of Object.keys(triggerToTargets)) {
        const attr = formContext.getAttribute(triggerAttr as any) as Xrm.Attributes.Attribute;

        if (!assertOrSkip(config, attr, `DPE.PolicyForm strict mode: trigger '${triggerAttr}' is not on the form.`)) {
            traceLog(config, `Skipping wiring for trigger '${triggerAttr}' (not on form)`);
            continue;
        }

        attr.addOnChange(() => {
            debouncer(async () => {
                try {
                    await evaluateAll(formContext, config, cache!);
                } catch (e) {
                    // eslint-disable-next-line no-console
                    console.error("DPE.PolicyForm evaluation failed:", e);
                }
            });
        });

        traceLog(config, `Wired onChange for trigger '${triggerAttr}'`);
    }
}

// Public API (manual refresh)
export async function reEvaluate(
    executionContext: Xrm.Events.EventContext,
    config: PolicyFormConfig
): Promise<void> {
    const validation = validatePolicyFormConfig(config);
    if (!validation.ok) throw new Error(validation.message);

    const formContext = executionContext.getFormContext();

    let cache = cacheByForm.get(formContext);
    if (!cache) {
        cache = {};
        cacheByForm.set(formContext, cache);
    }

    try {
        await evaluateAll(formContext, config, cache);
    } catch (e) {
        // eslint-disable-next-line no-console
        console.error("DPE.PolicyForm reEvaluate failed:", e);
    }
}

// Test-only: reset module singletons so state doesn't leak between vitest cases
export function __resetPolicyFormForTests(): void {
    wiredForms = new WeakSet<any>();
    cacheByForm = new WeakMap<any, Record<string, any>>();
}
