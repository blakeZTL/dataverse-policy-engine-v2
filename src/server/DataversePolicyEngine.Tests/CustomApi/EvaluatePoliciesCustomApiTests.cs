using DataversePolicyEngine.Core.Model;
using DataversePolicyEngine.CustomApi;
using FakeXrmEasy.Plugins;
using Microsoft.Xrm.Sdk;

namespace DataversePolicyEngine.Tests.CustomApi
{
    [TestClass]
    public class EvaluatePoliciesCustomApiTests : FakeXrmEasyTestBase
    {
        [TestMethod]
        public void Returns_NotAllowed_True_WhenPolicyMatches()
        {
            // Policy: account.name NotAllowed=true when statuscode == 1
            var ruleId = Guid.NewGuid();

            var rule = new Entity("dpe_policyrule", ruleId)
            {
                ["dpe_targetentitylogicalname"] = "account",
                ["dpe_targetattributelogicalname"] = "name",
                ["dpe_triggerattributelogicalname"] = "statuscode",
                ["dpe_policytype"] = new OptionSetValue(OptionSetMap.PolicyType_NotAllowed),
                ["dpe_result"] = true,
                ["dpe_sequence"] = 10,
                ["statecode"] = new OptionSetValue(0)
            };

            var condition = new Entity("dpe_policycondition", Guid.NewGuid())
            {
                ["dpe_policyrule"] = new EntityReference("dpe_policyrule", ruleId),
                ["dpe_sequence"] = 10,
                ["dpe_operator"] = new OptionSetValue(OptionSetMap.Operator_Equals),
                ["dpe_valuetype"] = new OptionSetValue(OptionSetMap.ValueType_OptionSet),
                ["dpe_valueoptionsetvalue"] = 1
            };

            _context.Initialize([rule, condition]);

            var pluginCtx = _context.GetDefaultPluginContext();
            pluginCtx.MessageName = "dpe_EvaluatePolicies";

            pluginCtx.InputParameters["EntityLogicalName"] = "account";
            pluginCtx.InputParameters["TargetAttributeLogicalName"] = "name";
            pluginCtx.InputParameters["TriggerAttributeLogicalName"] = "statuscode";
            pluginCtx.InputParameters["TriggerOptionSetValue"] = 1;

            _context.ExecutePluginWith<EvaluatePoliciesCustomApi>(pluginCtx);

            Assert.AreEqual(true, pluginCtx.OutputParameters["NotAllowed"]);
            Assert.AreEqual(true, pluginCtx.OutputParameters["Visible"]); // default true (no visible rule)
            Assert.AreEqual(false, pluginCtx.OutputParameters["Required"]); // default false (no required rule)
        }

        [TestMethod]
        public void Returns_Required_True_WhenPolicyMatches()
        {
            var ruleId = Guid.NewGuid();

            var rule = new Entity("dpe_policyrule", ruleId)
            {
                ["dpe_targetentitylogicalname"] = "account",
                ["dpe_targetattributelogicalname"] = "telephone1",
                ["dpe_triggerattributelogicalname"] = "statuscode",
                ["dpe_policytype"] = new OptionSetValue(OptionSetMap.PolicyType_Required),
                ["dpe_result"] = true,
                ["dpe_sequence"] = 10,
                ["statecode"] = new OptionSetValue(0)
            };

            var condition = new Entity("dpe_policycondition", Guid.NewGuid())
            {
                ["dpe_policyrule"] = new EntityReference("dpe_policyrule", ruleId),
                ["dpe_sequence"] = 10,
                ["dpe_operator"] = new OptionSetValue(OptionSetMap.Operator_Equals),
                ["dpe_valuetype"] = new OptionSetValue(OptionSetMap.ValueType_OptionSet),
                ["dpe_valueoptionsetvalue"] = 1
            };

            _context.Initialize([rule, condition]);

            var pluginCtx = _context.GetDefaultPluginContext();
            pluginCtx.MessageName = "dpe_EvaluatePolicies";

            pluginCtx.InputParameters["EntityLogicalName"] = "account";
            pluginCtx.InputParameters["TargetAttributeLogicalName"] = "telephone1";
            pluginCtx.InputParameters["TriggerAttributeLogicalName"] = "statuscode";
            pluginCtx.InputParameters["TriggerOptionSetValue"] = 1;

            _context.ExecutePluginWith<EvaluatePoliciesCustomApi>(pluginCtx);

            Assert.AreEqual(true, pluginCtx.OutputParameters["Required"]);
            Assert.AreEqual(false, pluginCtx.OutputParameters["NotAllowed"]);
            Assert.AreEqual(true, pluginCtx.OutputParameters["Visible"]);
        }

    }
}
