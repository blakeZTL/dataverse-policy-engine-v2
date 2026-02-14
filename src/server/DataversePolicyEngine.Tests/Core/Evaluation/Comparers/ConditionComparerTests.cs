using DataversePolicyEngine.Core.Evaluation.Comparers;
using DataversePolicyEngine.Core.Model;
using Microsoft.Xrm.Sdk;

namespace DataversePolicyEngine.Tests.Core.Evaluation.Comparers
{
    [TestClass]
    public class ConditionComparerTests
    {
        private const string TriggerAttr = "statuscode";

        [TestMethod]
        public void Matches_UsesTargetValueOverPreImage()
        {
            var condition = NewEqualsOptionSetCondition(1);

            var target = new Entity("account");
            target[TriggerAttr] = new OptionSetValue(1);

            var pre = new Entity("account");
            pre[TriggerAttr] = new OptionSetValue(999);

            Assert.IsTrue(ConditionComparer.Matches(condition, TriggerAttr, target, pre));
        }

        [TestMethod]
        public void Matches_FallsBackToPreImageWhenTargetMissingAttribute()
        {
            var condition = NewEqualsOptionSetCondition(1);

            var target = new Entity("account"); // no statuscode in target

            var pre = new Entity("account");
            pre[TriggerAttr] = new OptionSetValue(1);

            Assert.IsTrue(ConditionComparer.Matches(condition, TriggerAttr, target, pre));
        }

        [TestMethod]
        public void Matches_Equals_Works()
        {
            var condition = NewEqualsStringCondition("active");

            var target = new Entity("account");
            target[TriggerAttr] = "ACTIVE";

            Assert.IsTrue(ConditionComparer.Matches(condition, TriggerAttr, target, null));
        }

        [TestMethod]
        public void Matches_NotEquals_Works()
        {
            var condition = new Entity("dpe_policycondition")
            {
                ["dpe_operator"] = new OptionSetValue(OptionSetMap.Operator_NotEquals),
                ["dpe_valuetype"] = new OptionSetValue(OptionSetMap.ValueType_String),
                ["dpe_valuestring"] = "x"
            };

            var target = new Entity("account");
            target[TriggerAttr] = "y";

            Assert.IsTrue(ConditionComparer.Matches(condition, TriggerAttr, target, null));
        }

        [TestMethod]
        public void Matches_IsNull_Works()
        {
            var condition = new Entity("dpe_policycondition")
            {
                ["dpe_operator"] = new OptionSetValue(OptionSetMap.Operator_IsNull)
            };

            var target = new Entity("account"); // attribute missing => treated as null
            Assert.IsTrue(ConditionComparer.Matches(condition, TriggerAttr, target, null));

            var target2 = new Entity("account");
            target2[TriggerAttr] = "not null";
            Assert.IsFalse(ConditionComparer.Matches(condition, TriggerAttr, target2, null));
        }

        [TestMethod]
        public void Matches_IsNotNull_Works()
        {
            var condition = new Entity("dpe_policycondition")
            {
                ["dpe_operator"] = new OptionSetValue(OptionSetMap.Operator_IsNotNull)
            };

            var target = new Entity("account");
            Assert.IsFalse(ConditionComparer.Matches(condition, TriggerAttr, target, null));

            var target2 = new Entity("account");
            target2[TriggerAttr] = "x";
            Assert.IsTrue(ConditionComparer.Matches(condition, TriggerAttr, target2, null));
        }

        [TestMethod]
        public void Matches_WhenOperatorMissing_ReturnsFalse()
        {
            var condition = new Entity("dpe_policycondition"); // no dpe_operator
            var target = new Entity("account");
            target[TriggerAttr] = "x";

            Assert.IsFalse(ConditionComparer.Matches(condition, TriggerAttr, target, null));
        }

        private static Entity NewEqualsStringCondition(string value)
        {
            return new Entity("dpe_policycondition")
            {
                ["dpe_operator"] = new OptionSetValue(OptionSetMap.Operator_Equals),
                ["dpe_valuetype"] = new OptionSetValue(OptionSetMap.ValueType_String),
                ["dpe_valuestring"] = value
            };
        }

        private static Entity NewEqualsOptionSetCondition(int optionValue)
        {
            return new Entity("dpe_policycondition")
            {
                ["dpe_operator"] = new OptionSetValue(OptionSetMap.Operator_Equals),
                ["dpe_valuetype"] = new OptionSetValue(OptionSetMap.ValueType_OptionSet),
                ["dpe_valueoptionsetvalue"] = optionValue
            };
        }
    }
}
