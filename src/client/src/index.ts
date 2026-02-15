import * as PolicyClient from "./policyClient";
import * as PolicyForm from "./policyForm";
import * as ValidatePolicyFormConfig from "./configValidator";
import "../src/index";


// attach to window for Dataverse
const g = globalThis as any;
g.DPE = g.DPE || {};
g.DPE.PolicyClient = PolicyClient;
g.DPE.PolicyForm = PolicyForm;
g.DPE.ValidatePolicyFormConfig = ValidatePolicyFormConfig;


export { PolicyClient, PolicyForm, ValidatePolicyFormConfig };
