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

        [TestMethod]
        public void Throws_When_EntityLogicalName_Missing()
        {
            var pluginCtx = _context.GetDefaultPluginContext();
            pluginCtx.MessageName = "dpe_EvaluatePolicies";

            pluginCtx.InputParameters["TargetAttributeLogicalName"] = "name";
            pluginCtx.InputParameters["TriggerAttributeLogicalName"] = "statuscode";
            pluginCtx.InputParameters["TriggerOptionSetValue"] = 1;

            Assert.ThrowsException<InvalidPluginExecutionException>(() =>
                _context.ExecutePluginWith<EvaluatePoliciesCustomApi>(pluginCtx)
            );
        }

        [TestMethod]
        public void Throws_When_TargetAttributeLogicalName_Missing()
        {
            var pluginCtx = _context.GetDefaultPluginContext();
            pluginCtx.MessageName = "dpe_EvaluatePolicies";

            pluginCtx.InputParameters["EntityLogicalName"] = "account";
            pluginCtx.InputParameters["TriggerAttributeLogicalName"] = "statuscode";
            pluginCtx.InputParameters["TriggerOptionSetValue"] = 1;

            Assert.ThrowsException<InvalidPluginExecutionException>(() =>
                _context.ExecutePluginWith<EvaluatePoliciesCustomApi>(pluginCtx)
            );
        }

        [TestMethod]
        public void Throws_When_TriggerAttributeLogicalName_Missing()
        {
            var pluginCtx = _context.GetDefaultPluginContext();
            pluginCtx.MessageName = "dpe_EvaluatePolicies";

            pluginCtx.InputParameters["EntityLogicalName"] = "account";
            pluginCtx.InputParameters["TargetAttributeLogicalName"] = "telephone1";
            
            pluginCtx.InputParameters["TriggerOptionSetValue"] = 1;

            Assert.ThrowsException<InvalidPluginExecutionException>(() =>
                _context.ExecutePluginWith<EvaluatePoliciesCustomApi>(pluginCtx)
            );
        }

        [TestMethod]
        public void Uses_Boolean_Trigger_WhenProvided()
        {
            var ruleId = Guid.NewGuid();

            var rule = new Entity("dpe_policyrule", ruleId)
            {
                ["dpe_targetentitylogicalname"] = "account",
                ["dpe_targetattributelogicalname"] = "name",
                ["dpe_triggerattributelogicalname"] = "donotemail",
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
                ["dpe_valuetype"] = new OptionSetValue(OptionSetMap.ValueType_Boolean),
                ["dpe_valueboolean"] = true
            };

            _context.Initialize([rule, condition]);

            var pluginCtx = _context.GetDefaultPluginContext();
            pluginCtx.MessageName = "dpe_EvaluatePolicies";
            pluginCtx.InputParameters["EntityLogicalName"] = "account";
            pluginCtx.InputParameters["TargetAttributeLogicalName"] = "name";
            pluginCtx.InputParameters["TriggerAttributeLogicalName"] = "donotemail";
            pluginCtx.InputParameters["TriggerBoolean"] = true;

            _context.ExecutePluginWith<EvaluatePoliciesCustomApi>(pluginCtx);

            Assert.AreEqual(true, pluginCtx.OutputParameters["NotAllowed"]);
        }

        [TestMethod]
        public void Trigger_Precedence_Lookup_Wins_Over_OptionSet()
        {
            var ruleId = Guid.NewGuid();

            // Rule expects lookup match (so if lookup wins, NotAllowed true)
            var rule = new Entity("dpe_policyrule", ruleId)
            {
                ["dpe_targetentitylogicalname"] = "account",
                ["dpe_targetattributelogicalname"] = "name",
                ["dpe_triggerattributelogicalname"] = "primarycontactid",
                ["dpe_policytype"] = new OptionSetValue(OptionSetMap.PolicyType_NotAllowed),
                ["dpe_result"] = true,
                ["dpe_sequence"] = 10,
                ["statecode"] = new OptionSetValue(0)
            };

            var contactId = Guid.NewGuid();
            var condition = new Entity("dpe_policycondition", Guid.NewGuid())
            {
                ["dpe_policyrule"] = new EntityReference("dpe_policyrule", ruleId),
                ["dpe_sequence"] = 10,
                ["dpe_operator"] = new OptionSetValue(OptionSetMap.Operator_Equals),
                ["dpe_valuetype"] = new OptionSetValue(OptionSetMap.ValueType_Lookup),
                ["dpe_valuelookuplogicalname"] = "contact",
                ["dpe_valuelookupid"] = contactId
            };

            _context.Initialize([rule, condition]);

            var pluginCtx = _context.GetDefaultPluginContext();
            pluginCtx.MessageName = "dpe_EvaluatePolicies";
            pluginCtx.InputParameters["EntityLogicalName"] = "account";
            pluginCtx.InputParameters["TargetAttributeLogicalName"] = "name";
            pluginCtx.InputParameters["TriggerAttributeLogicalName"] = "primarycontactid";

            // Provide BOTH lookup and optionset, lookup should win
            pluginCtx.InputParameters["TriggerLookupLogicalName"] = "contact";
            pluginCtx.InputParameters["TriggerLookupId"] = contactId;
            pluginCtx.InputParameters["TriggerOptionSetValue"] = 999; // should be ignored

            _context.ExecutePluginWith<EvaluatePoliciesCustomApi>(pluginCtx);

            Assert.AreEqual(true, pluginCtx.OutputParameters["NotAllowed"]);
        }

        [TestMethod]
        public void NoTriggerProvided_Allows_IsNull_ToMatch()
        {
            var ruleId = Guid.NewGuid();

            var rule = new Entity("dpe_policyrule", ruleId)
            {
                ["dpe_targetentitylogicalname"] = "account",
                ["dpe_targetattributelogicalname"] = "name",
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
                ["dpe_operator"] = new OptionSetValue(OptionSetMap.Operator_IsNull),
                ["dpe_valuetype"] = new OptionSetValue(OptionSetMap.ValueType_OptionSet) // or whatever you require for IsNull
            };

            _context.Initialize([rule, condition]);

            var pluginCtx = _context.GetDefaultPluginContext();
            pluginCtx.MessageName = "dpe_EvaluatePolicies";
            pluginCtx.InputParameters["EntityLogicalName"] = "account";
            pluginCtx.InputParameters["TargetAttributeLogicalName"] = "name";
            pluginCtx.InputParameters["TriggerAttributeLogicalName"] = "statuscode";
            // IMPORTANT: do NOT provide any TriggerX param

            _context.ExecutePluginWith<EvaluatePoliciesCustomApi>(pluginCtx);

            Assert.AreEqual(true, pluginCtx.OutputParameters["Required"]);
        }

        [TestMethod]
        public void Ignores_Inactive_Rules()
        {
            var ruleId = Guid.NewGuid();

            var rule = new Entity("dpe_policyrule", ruleId)
            {
                ["dpe_targetentitylogicalname"] = "account",
                ["dpe_targetattributelogicalname"] = "name",
                ["dpe_triggerattributelogicalname"] = "statuscode",
                ["dpe_policytype"] = new OptionSetValue(OptionSetMap.PolicyType_NotAllowed),
                ["dpe_result"] = true,
                ["dpe_sequence"] = 10,
                ["statecode"] = new OptionSetValue(1) // inactive
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

            Assert.AreEqual(false, pluginCtx.OutputParameters["NotAllowed"]); // default
            Assert.AreEqual(true, pluginCtx.OutputParameters["Visible"]);     // default
            Assert.AreEqual(false, pluginCtx.OutputParameters["Required"]);   // default
        }

        [TestMethod]
        public void Returns_ResultsJson_With_One_Result_Per_Target()
        {
            // Policy 1: name NotAllowed true when statuscode == 1
            var ruleId1 = Guid.NewGuid();
            var rule1 = new Entity("dpe_policyrule", ruleId1)
            {
                ["dpe_targetentitylogicalname"] = "account",
                ["dpe_targetattributelogicalname"] = "name",
                ["dpe_triggerattributelogicalname"] = "statuscode",
                ["dpe_policytype"] = new OptionSetValue(OptionSetMap.PolicyType_NotAllowed),
                ["dpe_result"] = true,
                ["dpe_sequence"] = 10,
                ["statecode"] = new OptionSetValue(0)
            };
            var cond1 = new Entity("dpe_policycondition", Guid.NewGuid())
            {
                ["dpe_policyrule"] = new EntityReference("dpe_policyrule", ruleId1),
                ["dpe_sequence"] = 10,
                ["dpe_operator"] = new OptionSetValue(OptionSetMap.Operator_Equals),
                ["dpe_valuetype"] = new OptionSetValue(OptionSetMap.ValueType_OptionSet),
                ["dpe_valueoptionsetvalue"] = 1
            };

            // Policy 2: telephone1 Required true when statuscode == 1
            var ruleId2 = Guid.NewGuid();
            var rule2 = new Entity("dpe_policyrule", ruleId2)
            {
                ["dpe_targetentitylogicalname"] = "account",
                ["dpe_targetattributelogicalname"] = "telephone1",
                ["dpe_triggerattributelogicalname"] = "statuscode",
                ["dpe_policytype"] = new OptionSetValue(OptionSetMap.PolicyType_Required),
                ["dpe_result"] = true,
                ["dpe_sequence"] = 10,
                ["statecode"] = new OptionSetValue(0)
            };
            var cond2 = new Entity("dpe_policycondition", Guid.NewGuid())
            {
                ["dpe_policyrule"] = new EntityReference("dpe_policyrule", ruleId2),
                ["dpe_sequence"] = 10,
                ["dpe_operator"] = new OptionSetValue(OptionSetMap.Operator_Equals),
                ["dpe_valuetype"] = new OptionSetValue(OptionSetMap.ValueType_OptionSet),
                ["dpe_valueoptionsetvalue"] = 1
            };

            _context.Initialize([rule1, cond1, rule2, cond2]);

            var pluginCtx = _context.GetDefaultPluginContext();
            pluginCtx.MessageName = "dpe_EvaluatePolicies";

            pluginCtx.InputParameters["EntityLogicalName"] = "account";
            pluginCtx.InputParameters["TriggerAttributeLogicalName"] = "statuscode";
            pluginCtx.InputParameters["TriggerOptionSetValue"] = 1;

            // NEW batched input
            pluginCtx.InputParameters["TargetAttributeLogicalNames"] = new[] { "name", "telephone1" };

            _context.ExecutePluginWith<EvaluatePoliciesCustomApi>(pluginCtx);

            Assert.IsTrue(pluginCtx.OutputParameters.Contains("ResultsJson"));
            var resultsJson = (string)pluginCtx.OutputParameters["ResultsJson"];
            Assert.IsFalse(string.IsNullOrWhiteSpace(resultsJson));

            // Cheap verification without a JSON lib: just ensure both targets appear
            StringAssert.Contains(resultsJson, "\"Target\":\"name\"");
            StringAssert.Contains(resultsJson, "\"Target\":\"telephone1\"");
        }


    }
}
