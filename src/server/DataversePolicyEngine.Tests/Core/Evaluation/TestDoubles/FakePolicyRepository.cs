using DataversePolicyEngine.Core.Data;
using Microsoft.Xrm.Sdk;

namespace DataversePolicyEngine.Tests.Core.Evaluation.TestDoubles
{
    internal sealed class FakePolicyRepository : IPolicyRepository
    {
        private readonly List<Entity> _rules = new List<Entity>();
        private readonly Dictionary<Guid, List<Entity>> _conditionsByRuleId =
            new Dictionary<Guid, List<Entity>>();

        public void AddRule(Entity rule, params Entity[] conditions)
        {
            _rules.Add(rule);

            if (!_conditionsByRuleId.ContainsKey(rule.Id))
                _conditionsByRuleId[rule.Id] = new List<Entity>();

            if (conditions != null)
                _conditionsByRuleId[rule.Id].AddRange(conditions);
        }

        public List<Entity> GetRules(
            IOrganizationService service,
            string entityLogicalName,
            string attributeLogicalName
        )
        {
            // For evaluator tests we assume repository returns what evaluator should process.
            // (Your real repository does filtering + ordering.)
            return new List<Entity>(_rules);
        }

        public List<Entity> GetConditions(IOrganizationService service, Guid ruleId)
        {
            if (_conditionsByRuleId.TryGetValue(ruleId, out var list))
                return new List<Entity>(list);

            return new List<Entity>();
        }
    }
}
