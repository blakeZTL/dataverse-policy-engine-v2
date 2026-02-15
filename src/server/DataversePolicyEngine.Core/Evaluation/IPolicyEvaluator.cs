using Microsoft.Xrm.Sdk;

namespace DataversePolicyEngine.Core.Evaluation
{
    public interface IPolicyEvaluator
    {
        PolicyDecision EvaluateAttribute(
            IOrganizationService service,
            string entityLogicalName,
            string attributeLogicalName,
            Entity target, // values being written (may or may not contain attribute)
            Entity preImage // may be null; or may not contain attribute
        );
    }
}
