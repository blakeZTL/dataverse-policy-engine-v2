import { beforeEach, afterEach, vi } from "vitest";
import { XrmMockGenerator } from "xrm-mock";

declare global {
    // eslint-disable-next-line no-var
    var Xrm: Xrm.XrmStatic;
}

beforeEach(() => {
    XrmMockGenerator.initialise();

    // Ensure WebApi exists
    (globalThis as any).Xrm.WebApi = {
        online: {
            execute: vi.fn()
        }
    };

    // Ensure entity name exists (some xrm-mock versions don’t set this reliably)
    const page: any = (globalThis as any).Xrm.Page;
    page.data = page.data || {};
    page.data.entity = page.data.entity || {};
    page.data.entity.getEntityName = page.data.entity.getEntityName || (() => "account");

    vi.spyOn(console, "error").mockImplementation(() => { });
});

afterEach(() => {
    vi.restoreAllMocks();
});

export { };
