using Microsoft.Xrm.Sdk;
using System;
using System.Collections.Generic;

namespace DataversePolicyEngine.Core.Data
{
    public interface IPolicyRepository
    {
        List<Entity> GetRules(
            IOrganizationService service,
            string entityLogicalName,
            string attributeLogicalName
        );
        List<Entity> GetConditions(IOrganizationService service, Guid ruleId);
    }
}
