import { XrmMockGenerator } from "xrm-mock";

export function makeExecutionContext(): Xrm.Events.EventContext {
  return {
    getFormContext: () => (Xrm.Page as any)
  } as any;
}

export function mockApiReturn(results: {
    Results: Array<{
        Target: string;
        Visible: boolean;
        Required: boolean;
        NotAllowed: boolean;
    }>;
}) {
    (Xrm.WebApi.online.execute as any).mockResolvedValue({
        json: async () => results
    });
}

export function getExecuteRequests(): any[] {
  return (Xrm.WebApi.online.execute as any).mock.calls.map((c: any[]) => c[0]);
}

export function setEntityName(name: string) {
  (Xrm.Page as any).data.entity.getEntityName = () => name;
}

export function addStringField(name: string, initial = "") {
  var attr = XrmMockGenerator.Attribute.createString(name, initial);
    XrmMockGenerator.Control.createString(attr);
}

export function addBooleanField(name: string, initial = false) {
  var attr = XrmMockGenerator.Attribute.createBoolean(name, initial);
    XrmMockGenerator.Control.createBoolean(attr);
}

export function addOptionSetField(name: string, initial = 0) {
  var attr = XrmMockGenerator.Attribute.createOptionSet(name, initial);
    XrmMockGenerator.Control.createOptionSet(attr);
}

export function addLookupField(name: string, lookup: { id: string; name: string; entityType: string }[] | null) {
  var attr = XrmMockGenerator.Attribute.createLookup(name, lookup as any);
    XrmMockGenerator.Control.createLookup(attr);
}

export function fireOnChange(attributeLogicalName: string) {
  const attr: any = (Xrm.Page as any).getAttribute(attributeLogicalName);
  if (!attr) throw new Error(`Attribute not found: ${attributeLogicalName}`);
  if (typeof attr.fireOnChange === "function") {
    attr.fireOnChange();
    return;
  }
  // Fallback: xrm-mock older shapes sometimes expose events differently
  const handlers = attr?.controls?.get?.(0)?._onChangeHandlers;
  if (handlers) handlers.forEach((h: any) => h());
}
