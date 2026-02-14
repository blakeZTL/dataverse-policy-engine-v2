using DataversePolicyEngine.Core.Evaluation.Comparers;
using DataversePolicyEngine.Core.Model;
using Microsoft.Xrm.Sdk;

namespace DataversePolicyEngine.Tests.Core.Evaluation.Comparers
{
    [TestClass]
    public class DataverseValueComparerTests
    {
        [TestMethod]
        public void Equals_String_IsCaseInsensitive()
        {
            var condition = new Entity("dpe_policycondition")
            {
                ["dpe_valuetype"] = new OptionSetValue(OptionSetMap.ValueType_String),
                ["dpe_valuestring"] = "ACTIVE"
            };

            var currentValue = "active";

            Assert.IsTrue(DataverseValueComparer.Equals(condition, currentValue));
        }

        [TestMethod]
        public void Equals_Number_MatchesDecimal()
        {
            var condition = new Entity("dpe_policycondition")
            {
                ["dpe_valuetype"] = new OptionSetValue(OptionSetMap.ValueType_Number),
                ["dpe_valuenumber"] = 10.5m
            };

            Assert.IsTrue(DataverseValueComparer.Equals(condition, 10.5m));
            Assert.IsTrue(DataverseValueComparer.Equals(condition, 10.5)); // double -> decimal
            Assert.IsFalse(DataverseValueComparer.Equals(condition, 10.6m));
        }

        [TestMethod]
        public void Equals_Boolean_Matches()
        {
            var condition = new Entity("dpe_policycondition")
            {
                ["dpe_valuetype"] = new OptionSetValue(OptionSetMap.ValueType_Boolean),
                ["dpe_valueboolean"] = true
            };

            Assert.IsTrue(DataverseValueComparer.Equals(condition, true));
            Assert.IsFalse(DataverseValueComparer.Equals(condition, false));
        }

        [TestMethod]
        public void Equals_OptionSet_MatchesByIntegerValue()
        {
            var condition = new Entity("dpe_policycondition")
            {
                ["dpe_valuetype"] = new OptionSetValue(OptionSetMap.ValueType_OptionSet),
                ["dpe_valueoptionsetvalue"] = 42
            };

            Assert.IsTrue(DataverseValueComparer.Equals(condition, new OptionSetValue(42)));
            Assert.IsFalse(DataverseValueComparer.Equals(condition, new OptionSetValue(43)));
        }

        [TestMethod]
        public void Equals_Lookup_MatchesByLogicalNameAndGuid()
        {
            var id = Guid.NewGuid();

            var condition = new Entity("dpe_policycondition")
            {
                ["dpe_valuetype"] = new OptionSetValue(OptionSetMap.ValueType_Lookup),
                ["dpe_valuelookuplogicalname"] = "account",
                ["dpe_valuelookupid"] = id
            };

            Assert.IsTrue(
                DataverseValueComparer.Equals(condition, new EntityReference("account", id))
            );
            Assert.IsFalse(
                DataverseValueComparer.Equals(condition, new EntityReference("contact", id))
            );
            Assert.IsFalse(
                DataverseValueComparer.Equals(
                    condition,
                    new EntityReference("account", Guid.NewGuid())
                )
            );
        }

        [TestMethod]
        public void Equals_WhenCurrentValueNull_ReturnsFalse()
        {
            var condition = new Entity("dpe_policycondition")
            {
                ["dpe_valuetype"] = new OptionSetValue(OptionSetMap.ValueType_String),
                ["dpe_valuestring"] = "x"
            };

            Assert.IsFalse(DataverseValueComparer.Equals(condition, null));
        }
    }
}
