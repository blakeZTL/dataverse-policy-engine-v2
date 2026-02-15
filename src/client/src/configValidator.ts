// src/configValidator.ts
import type { PolicyFormConfig } from "./types";

export type ValidationResult =
  | { ok: true }
  | { ok: false; message: string };

export function validatePolicyFormConfig(config: PolicyFormConfig | null | undefined): ValidationResult {
  if (!config) return { ok: false, message: "Config is required." };
  if (!Array.isArray(config.rules) || config.rules.length === 0) {
    return { ok: false, message: "config.rules must be a non-empty array." };
  }

  for (let i = 0; i < config.rules.length; i++) {
    const r = config.rules[i];
    if (!r) return { ok: false, message: `rules[${i}] is null/undefined.` };

    if (typeof r.target !== "string" || r.target.trim().length === 0) {
      return { ok: false, message: `rules[${i}].target is required.` };
    }

    if (typeof r.trigger !== "string" || r.trigger.trim().length === 0) {
      return { ok: false, message: `rules[${i}].trigger is required.` };
    }
  }

  if (config.debounceMs !== undefined && (typeof config.debounceMs !== "number" || config.debounceMs < 0)) {
    return { ok: false, message: "config.debounceMs must be a number >= 0." };
  }

  if (config.applyNotAllowedAsDisabled !== undefined && typeof config.applyNotAllowedAsDisabled !== "boolean") {
    return { ok: false, message: "config.applyNotAllowedAsDisabled must be a boolean." };
  }

  if (config.entityLogicalName !== undefined && (typeof config.entityLogicalName !== "string" || config.entityLogicalName.trim().length === 0)) {
    return { ok: false, message: "config.entityLogicalName must be a non-empty string if provided." };
  }

  return { ok: true };
}
