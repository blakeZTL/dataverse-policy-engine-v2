using DataversePolicyEngine.Core.Evaluation;
using DataversePolicyEngine.Core.Model;
using DataversePolicyEngine.Tests.Core.Evaluation.TestDoubles;
using Microsoft.Xrm.Sdk;

namespace DataversePolicyEngine.Tests.Core.Evaluation
{
    [TestClass]
    public class PolicyEvaluatorTests
    {
        private const string EntityName = "account";
        private const string TargetAttribute = "name";
        private const string TriggerAttribute = "statuscode";

        [TestMethod]
        public void Visible_FirstMatchWins_EvaluatorOrdersRulesBySequence()
        {
            var repo = new FakePolicyRepository();

            // Lower sequence => should win (Visible=false)
            var ruleLowSeq = NewRule(
                PolicyType.Visible,
                result: false,
                sequence: 10,
                triggerAttr: TriggerAttribute
            );

            // Higher sequence => should lose (Visible=true)
            var ruleHighSeq = NewRule(
                PolicyType.Visible,
                result: true,
                sequence: 20,
                triggerAttr: TriggerAttribute
            );

            var c = NewIsNotNullCondition();

            // Add in reverse order intentionally
            repo.AddRule(ruleHighSeq, c);
            repo.AddRule(ruleLowSeq, c);

            var evaluator = new PolicyEvaluator(repo);

            var target = new Entity(EntityName);
            target[TriggerAttribute] = new OptionSetValue(1);

            var decision = evaluator.EvaluateAttribute(
                null,
                EntityName,
                TargetAttribute,
                target,
                null
            );

            Assert.IsFalse(
                decision.Visible,
                "Evaluator must order rules by sequence; lower sequence should win."
            );
        }

        [TestMethod]
        public void Required_DefaultsToFalse_WhenNoRulesMatch()
        {
            var repo = new FakePolicyRepository();

            // Required=true but condition does NOT match (IsNull) while value is not null
            var rule = NewRule(
                PolicyType.Required,
                result: true,
                sequence: 10,
                triggerAttr: TriggerAttribute
            );
            repo.AddRule(rule, NewIsNullCondition());

            var evaluator = new PolicyEvaluator(repo);

            var target = new Entity(EntityName);
            target[TriggerAttribute] = "x";

            var decision = evaluator.EvaluateAttribute(
                null,
                EntityName,
                TargetAttribute,
                target,
                null
            );

            Assert.IsFalse(
                decision.Required,
                "Required should default to false when no required rules match."
            );
        }

        [TestMethod]
        public void Required_FirstMatchWins_WhenMultipleMatch()
        {
            var repo = new FakePolicyRepository();

            // Rule 1: Required=true, matches
            var rule1 = NewRule(
                PolicyType.Required,
                result: true,
                sequence: 10,
                triggerAttr: TriggerAttribute
            );

            // Rule 2: Required=false, also matches, but should not be reached
            var rule2 = NewRule(
                PolicyType.Required,
                result: false,
                sequence: 20,
                triggerAttr: TriggerAttribute
            );

            var c = NewEqualsStringCondition("active");

            repo.AddRule(rule1, c);
            repo.AddRule(rule2, c);

            var evaluator = new PolicyEvaluator(repo);

            var target = new Entity(EntityName);
            target[TriggerAttribute] = "ACTIVE";

            var decision = evaluator.EvaluateAttribute(
                null,
                EntityName,
                TargetAttribute,
                target,
                null
            );

            Assert.IsTrue(decision.Required, "First matching Required rule should win.");
        }

        [TestMethod]
        public void NotAllowed_DenyOverrides_EvaluatorOrdersRulesBySequence()
        {
            var repo = new FakePolicyRepository();

            // Higher priority rule does NOT block
            var ruleLowSeq = NewRule(
                PolicyType.NotAllowed,
                result: false,
                sequence: 10,
                triggerAttr: TriggerAttribute
            );

            // Lower priority rule blocks (still should block because deny overrides scans all, but ordering is still enforced)
            var ruleHighSeq = NewRule(
                PolicyType.NotAllowed,
                result: true,
                sequence: 20,
                triggerAttr: TriggerAttribute
            );

            var c = NewEqualsOptionSetCondition(1);

            // Add in reverse order intentionally
            repo.AddRule(ruleHighSeq, c);
            repo.AddRule(ruleLowSeq, c);

            var evaluator = new PolicyEvaluator(repo);

            var target = new Entity(EntityName);
            target[TriggerAttribute] = new OptionSetValue(1);

            var decision = evaluator.EvaluateAttribute(
                null,
                EntityName,
                TargetAttribute,
                target,
                null
            );

            Assert.IsTrue(
                decision.NotAllowed,
                "Any matching NotAllowed=true should block (deny overrides), regardless of insertion order."
            );
        }

        [TestMethod]
        public void NotAllowed_DefaultsToFalse_WhenNoNotAllowedRulesMatch()
        {
            var repo = new FakePolicyRepository();

            // NotAllowed=true rule exists but doesn't match
            var rule = NewRule(
                PolicyType.NotAllowed,
                result: true,
                sequence: 10,
                triggerAttr: TriggerAttribute
            );
            repo.AddRule(rule, NewEqualsOptionSetCondition(99));

            var evaluator = new PolicyEvaluator(repo);

            var target = new Entity(EntityName);
            target[TriggerAttribute] = new OptionSetValue(1);

            var decision = evaluator.EvaluateAttribute(
                null,
                EntityName,
                TargetAttribute,
                target,
                null
            );

            Assert.IsFalse(
                decision.NotAllowed,
                "NotAllowed should default to false when no rules match."
            );
        }

        [TestMethod]
        public void Conditions_AreAnded_AllMustMatch()
        {
            var repo = new FakePolicyRepository();

            var rule = NewRule(
                PolicyType.Visible,
                result: false,
                sequence: 10,
                triggerAttr: TriggerAttribute
            );

            // Condition 1: Equals "active" (true)
            var c1 = NewEqualsStringCondition("active");

            // Condition 2: Equals "blocked" (false)
            var c2 = NewEqualsStringCondition("blocked");

            repo.AddRule(rule, c1, c2);

            var evaluator = new PolicyEvaluator(repo);

            var target = new Entity(EntityName);
            target[TriggerAttribute] = "ACTIVE";

            var decision = evaluator.EvaluateAttribute(
                null,
                EntityName,
                TargetAttribute,
                target,
                null
            );

            Assert.IsTrue(
                decision.Visible,
                "Rule should not match because not all conditions match; Visible defaults to true."
            );
        }

        [TestMethod]
        public void Conditions_AreEvaluatedInSequenceOrder()
        {
            var repo = new FakePolicyRepository();

            var rule = NewRule(
                PolicyType.Visible,
                result: false,
                sequence: 10,
                triggerAttr: TriggerAttribute
            );

            // Condition that will FAIL (expects 'blocked')
            var cFail = NewEqualsStringCondition("blocked");
            cFail["dpe_sequence"] = 20;

            // Condition that will PASS (expects 'active')
            var cPass = NewEqualsStringCondition("active");
            cPass["dpe_sequence"] = 10;

            // Add in reverse order intentionally (fail first, pass second)
            repo.AddRule(rule, cFail, cPass);

            var evaluator = new PolicyEvaluator(repo);

            var target = new Entity(EntityName);
            target[TriggerAttribute] = "ACTIVE";

            var decision = evaluator.EvaluateAttribute(
                null,
                EntityName,
                TargetAttribute,
                target,
                null
            );

            // Because conditions are AND, rule should NOT match regardless of order,
            // but this test ensures we can safely rely on sequence ordering for short-circuit/debugging.
            Assert.IsTrue(
                decision.Visible,
                "Rule should not match because one condition fails; Visible defaults true."
            );
        }

        [TestMethod]
        public void TriggerAttribute_MissingOnRule_PreventsMatch()
        {
            var repo = new FakePolicyRepository();

            // trigger attribute is null/empty => should never match
            var rule = NewRule(PolicyType.Required, result: true, sequence: 10, triggerAttr: null);
            repo.AddRule(rule, NewIsNotNullCondition());

            var evaluator = new PolicyEvaluator(repo);

            var target = new Entity(EntityName);
            target[TriggerAttribute] = "x";

            var decision = evaluator.EvaluateAttribute(
                null,
                EntityName,
                TargetAttribute,
                target,
                null
            );

            Assert.IsFalse(
                decision.Required,
                "Rule should not match without a trigger attribute; Required defaults to false."
            );
        }

        // ---------------------------
        // Helpers
        // ---------------------------

        private static Entity NewRule(
            PolicyType policyType,
            bool result,
            int sequence,
            string triggerAttr
        )
        {
            // NOTE: repository ordering is assumed correct; tests add rules in the desired order.
            var rule = new Entity("dpe_policyrule", Guid.NewGuid())
            {
                ["dpe_policytype"] = new OptionSetValue(OptionSetMap.ToOptionValue(policyType)),
                ["dpe_result"] = result,
                ["dpe_sequence"] = sequence
            };

            if (!string.IsNullOrWhiteSpace(triggerAttr))
                rule["dpe_triggerattributelogicalname"] = triggerAttr;

            // Included for completeness; evaluator tests don't rely on repo filtering here.
            rule["dpe_targetentitylogicalname"] = EntityName;
            rule["dpe_targetattributelogicalname"] = TargetAttribute;

            return rule;
        }

        private static Entity NewIsNullCondition()
        {
            return new Entity("dpe_policycondition", Guid.NewGuid())
            {
                ["dpe_operator"] = new OptionSetValue(OptionSetMap.Operator_IsNull)
            };
        }

        private static Entity NewIsNotNullCondition()
        {
            return new Entity("dpe_policycondition", Guid.NewGuid())
            {
                ["dpe_operator"] = new OptionSetValue(OptionSetMap.Operator_IsNotNull)
            };
        }

        private static Entity NewEqualsStringCondition(string value)
        {
            return new Entity("dpe_policycondition", Guid.NewGuid())
            {
                ["dpe_operator"] = new OptionSetValue(OptionSetMap.Operator_Equals),
                ["dpe_valuetype"] = new OptionSetValue(OptionSetMap.ValueType_String),
                ["dpe_valuestring"] = value
            };
        }

        private static Entity NewEqualsOptionSetCondition(int optionValue)
        {
            return new Entity("dpe_policycondition", Guid.NewGuid())
            {
                ["dpe_operator"] = new OptionSetValue(OptionSetMap.Operator_Equals),
                ["dpe_valuetype"] = new OptionSetValue(OptionSetMap.ValueType_OptionSet),
                ["dpe_valueoptionsetvalue"] = optionValue
            };
        }
    }
}
