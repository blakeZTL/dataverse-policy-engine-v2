using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using System;
using System.Collections.Generic;
using System.Linq;

namespace DataversePolicyEngine.Core.Data
{
    public class PolicyRepository : IPolicyRepository
    {
        public List<Entity> GetRules(
            IOrganizationService service,
            string entityLogicalName,
            string attributeLogicalName
        )
        {
            var query = new QueryExpression("dpe_policyrule") { ColumnSet = new ColumnSet(true) };

            query.Criteria.AddCondition(
                "dpe_targetentitylogicalname",
                ConditionOperator.Equal,
                entityLogicalName
            );
            query.Criteria.AddCondition(
                "dpe_targetattributelogicalname",
                ConditionOperator.Equal,
                attributeLogicalName
            );
            query.Criteria.AddCondition("statecode", ConditionOperator.Equal, 0); // active

            query.AddOrder("dpe_sequence", OrderType.Ascending);

            return service.RetrieveMultiple(query).Entities.ToList();
        }

        public List<Entity> GetConditions(IOrganizationService service, Guid ruleId)
        {
            var query = new QueryExpression("dpe_policycondition")
            {
                ColumnSet = new ColumnSet(true)
            };

            query.Criteria.AddCondition("dpe_policyruleid", ConditionOperator.Equal, ruleId);
            query.AddOrder("dpe_sequence", OrderType.Ascending);

            return service.RetrieveMultiple(query).Entities.ToList();
        }
    }
}
