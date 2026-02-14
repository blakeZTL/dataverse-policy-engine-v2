using DataversePolicyEngine.Core.Data;
using DataversePolicyEngine.Core.Model;
using Microsoft.Xrm.Sdk;
using System.Collections.Generic;
using System.Linq;

namespace DataversePolicyEngine.Core.Evaluation
{
    public sealed class PolicyEvaluator : IPolicyEvaluator
    {
        private readonly IPolicyRepository _repo;

        public PolicyEvaluator(IPolicyRepository repo)
        {
            _repo = repo;
        }

        public PolicyDecision EvaluateAttribute(
            IOrganizationService service,
            string entityLogicalName,
            string attributeLogicalName,
            Entity target,
            Entity preImage
        )
        {
            var decision = new PolicyDecision();

            var rules = _repo.GetRules(service, entityLogicalName, attributeLogicalName);

            // Visible (first match wins; default true)
            decision.Visible = EvaluateFirstMatch(
                service,
                rules,
                PolicyType.Visible,
                target,
                preImage,
                defaultValue: true,
                onMatchRuleId: id => decision.VisibleMatchedRuleId = id
            );

            // Required (first match wins; default false)
            decision.Required = EvaluateFirstMatch(
                service,
                rules,
                PolicyType.Required,
                target,
                preImage,
                defaultValue: false,
                onMatchRuleId: id => decision.RequiredMatchedRuleId = id
            );

            // NotAllowed (deny overrides; default false)
            decision.NotAllowed = EvaluateDenyOverrides(
                service,
                rules,
                PolicyType.NotAllowed,
                target,
                preImage,
                onMatchRuleId: id => decision.NotAllowedMatchedRuleId = id
            );

            return decision;
        }

        private bool EvaluateFirstMatch(
            IOrganizationService service,
            List<Entity> allRules,
            PolicyType policyType,
            Entity target,
            Entity preImage,
            bool defaultValue,
            System.Action<string> onMatchRuleId
        )
        {
            int policyOptionValue = OptionSetMap.ToOptionValue(policyType);

            var rules = allRules.Where(
                r =>
                    r.GetAttributeValue<OptionSetValue>("dpe_policytype")?.Value
                    == policyOptionValue
            );

            foreach (var rule in rules)
            {
                var conditions = _repo.GetConditions(service, rule.Id);
                if (AllConditionsMatch(rule, conditions, target, preImage))
                {
                    onMatchRuleId?.Invoke(rule.Id.ToString());
                    return rule.GetAttributeValue<bool>("dpe_result");
                }
            }

            return defaultValue;
        }

        private bool EvaluateDenyOverrides(
            IOrganizationService service,
            List<Entity> allRules,
            PolicyType policyType,
            Entity target,
            Entity preImage,
            System.Action<string> onMatchRuleId
        )
        {
            int policyOptionValue = OptionSetMap.ToOptionValue(policyType);

            var rules = allRules.Where(
                r =>
                    r.GetAttributeValue<OptionSetValue>("dpe_policytype")?.Value
                    == policyOptionValue
            );

            foreach (var rule in rules)
            {
                var conditions = _repo.GetConditions(service, rule.Id);

                if (
                    AllConditionsMatch(rule, conditions, target, preImage)
                    && rule.GetAttributeValue<bool>("dpe_result")
                )
                {
                    onMatchRuleId?.Invoke(rule.Id.ToString());
                    return true;
                }
            }

            return false;
        }

        private bool AllConditionsMatch(
            Entity rule,
            List<Entity> conditions,
            Entity target,
            Entity preImage
        )
        {
            var triggerAttr = rule.GetAttributeValue<string>("dpe_triggerattributelogicalname");
            if (string.IsNullOrWhiteSpace(triggerAttr))
                return false;

            foreach (var c in conditions)
            {
                if (!Comparers.ConditionComparer.Matches(c, triggerAttr, target, preImage))
                    return false;
            }
            return true;
        }
    }
}
