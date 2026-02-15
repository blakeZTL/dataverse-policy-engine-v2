import { describe, it, expect } from "vitest";
import { validatePolicyFormConfig } from "../src/configValidator";

describe("validatePolicyFormConfig", () => {
    it("fails when config is missing", () => {
        expect(validatePolicyFormConfig(null).ok).toBe(false);
        expect(validatePolicyFormConfig(undefined).ok).toBe(false);
    });

    it("fails when rules missing/empty", () => {
        expect(validatePolicyFormConfig({} as any).ok).toBe(false);
        expect(validatePolicyFormConfig({ rules: [] } as any).ok).toBe(false);
    });

    it("fails when a rule is missing target/trigger", () => {
        expect(validatePolicyFormConfig({ rules: [{ target: "name" }] } as any).ok).toBe(false);
        expect(validatePolicyFormConfig({ rules: [{ trigger: "statuscode" }] } as any).ok).toBe(false);
        expect(validatePolicyFormConfig({ rules: [{ target: " ", trigger: "x" }] } as any).ok).toBe(false);
        expect(validatePolicyFormConfig({ rules: [{ target: "x", trigger: " " }] } as any).ok).toBe(false);
    });

    it("fails on invalid debounceMs", () => {
        expect(validatePolicyFormConfig({ rules: [{ target: "x", trigger: "y" }], debounceMs: -1 } as any).ok).toBe(false);
    });

    it("passes for a minimal valid config", () => {
        expect(
            validatePolicyFormConfig({
                rules: [{ target: "name", trigger: "statuscode" }]
            }).ok
        ).toBe(true);
    });
});
