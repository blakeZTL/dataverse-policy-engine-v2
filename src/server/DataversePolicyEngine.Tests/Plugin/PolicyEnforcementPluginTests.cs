using DataversePolicyEngine.Core.Model;
using DataversePolicyEngine.Plugin;
using FakeXrmEasy;
using FakeXrmEasy.Plugins;
using Microsoft.Xrm.Sdk;

namespace DataversePolicyEngine.Tests.Plugin
{
    [TestClass]
    public class PolicyEnforcementPluginTests : FakeXrmEasyTestBase
    {
        [TestMethod]

        public void Update_NotAllowed_BlocksChange_WhenPolicyMatches()
        {
            var ctx = _context;

            // Arrange: policy says NotAllowed=true when trigger statuscode == 1
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
                ["dpe_policyruleid"] = new EntityReference("dpe_policyrule", ruleId),
                ["dpe_sequence"] = 10,
                ["dpe_operator"] = new OptionSetValue(OptionSetMap.Operator_Equals),
                ["dpe_valuetype"] = new OptionSetValue(OptionSetMap.ValueType_OptionSet),
                ["dpe_valueoptionsetvalue"] = 1
            };

            ctx.Initialize([rule, condition]);

            var target = new Entity("account", Guid.NewGuid())
            {
                ["name"] = "NEW NAME",
                ["statuscode"] = new OptionSetValue(1)
            };

            var preImage = new Entity("account", target.Id)
            {
                ["name"] = "OLD NAME",
                ["statuscode"] = new OptionSetValue(1)
            };

            var pluginCtx = ctx.GetDefaultPluginContext();
            pluginCtx.MessageName = "Update";
            pluginCtx.InputParameters["Target"] = target;
            pluginCtx.PreEntityImages["PreImage"] = preImage;

            // Act -> should throw           

            var ex = Assert.ThrowsException<InvalidPluginExecutionException>(() =>
            {
                ctx.ExecutePluginWith<PolicyEnforcementPlugin>(pluginCtx);
            });

            Assert.IsTrue(ex.Message.Contains("not allowed"), "Exception message should contain 'not allowed'");
        }

        [TestMethod]
        public void Update_Required_BlocksSettingNull_WhenPolicyMatches()
        {
            var ctx = _context;

            // Required=true when trigger statuscode == 1
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
                ["dpe_policyruleid"] = new EntityReference("dpe_policyrule", ruleId),
                ["dpe_sequence"] = 10,
                ["dpe_operator"] = new OptionSetValue(OptionSetMap.Operator_Equals),
                ["dpe_valuetype"] = new OptionSetValue(OptionSetMap.ValueType_OptionSet),
                ["dpe_valueoptionsetvalue"] = 1
            };

            ctx.Initialize([rule, condition]);

            var target = new Entity("account", Guid.NewGuid())
            {
                ["telephone1"] = null, // user tries to clear it
                ["statuscode"] = new OptionSetValue(1)
            };

            var preImage = new Entity("account", target.Id)
            {
                ["telephone1"] = "555-5555",
                ["statuscode"] = new OptionSetValue(1)
            };

            var pluginCtx = ctx.GetDefaultPluginContext();
            pluginCtx.MessageName = "Update";
            pluginCtx.InputParameters["Target"] = target;
            pluginCtx.PreEntityImages["PreImage"] = preImage;

            // Act -> should throw
            var ex = Assert.ThrowsException<InvalidPluginExecutionException>(() =>
            {
                ctx.ExecutePluginWith<PolicyEnforcementPlugin>(pluginCtx);
            });

            Assert.IsTrue(ex.Message.Contains("required"), "Exception message should contain 'required'");

        }

        [TestMethod]
        public void Update_NotAllowed_DoesNotBlock_WhenValueUnchanged()
        {
            var ctx = _context;

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
                ["dpe_policyruleid"] = new EntityReference("dpe_policyrule", ruleId),
                ["dpe_sequence"] = 10,
                ["dpe_operator"] = new OptionSetValue(OptionSetMap.Operator_Equals),
                ["dpe_valuetype"] = new OptionSetValue(OptionSetMap.ValueType_OptionSet),
                ["dpe_valueoptionsetvalue"] = 1
            };

            ctx.Initialize([rule, condition]);

            var id = Guid.NewGuid();

            var target = new Entity("account", id)
            {
                ["name"] = "SAME",
                ["statuscode"] = new OptionSetValue(1)
            };

            var preImage = new Entity("account", id)
            {
                ["name"] = "SAME",
                ["statuscode"] = new OptionSetValue(1)
            };

            var pluginCtx = ctx.GetDefaultPluginContext();
            pluginCtx.MessageName = "Update";
            pluginCtx.InputParameters["Target"] = target;
            pluginCtx.PreEntityImages["PreImage"] = preImage;

            // Should NOT throw
            ctx.ExecutePluginWith<PolicyEnforcementPlugin>(pluginCtx);
        }

        [TestMethod]
        public void Update_NotAllowed_DoesNotBlock_WhenPolicyDoesNotMatch()
        {
            var ctx = _context;

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

            // condition expects statuscode == 99, but we set it to 1
            var condition = new Entity("dpe_policycondition", Guid.NewGuid())
            {
                ["dpe_policyruleid"] = new EntityReference("dpe_policyrule", ruleId),
                ["dpe_sequence"] = 10,
                ["dpe_operator"] = new OptionSetValue(OptionSetMap.Operator_Equals),
                ["dpe_valuetype"] = new OptionSetValue(OptionSetMap.ValueType_OptionSet),
                ["dpe_valueoptionsetvalue"] = 99
            };

            ctx.Initialize([rule, condition]);

            var id = Guid.NewGuid();

            var target = new Entity("account", id)
            {
                ["name"] = "NEW NAME",
                ["statuscode"] = new OptionSetValue(1)
            };

            var preImage = new Entity("account", id)
            {
                ["name"] = "OLD NAME",
                ["statuscode"] = new OptionSetValue(1)
            };

            var pluginCtx = ctx.GetDefaultPluginContext();
            pluginCtx.MessageName = "Update";
            pluginCtx.InputParameters["Target"] = target;
            pluginCtx.PreEntityImages["PreImage"] = preImage;

            // Should NOT throw
            ctx.ExecutePluginWith<PolicyEnforcementPlugin>(pluginCtx);
        }

        [TestMethod]
        public void Update_Required_Allows_WhenValueProvided()
        {
            var ctx = _context;

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
                ["dpe_policyruleid"] = new EntityReference("dpe_policyrule", ruleId),
                ["dpe_sequence"] = 10,
                ["dpe_operator"] = new OptionSetValue(OptionSetMap.Operator_Equals),
                ["dpe_valuetype"] = new OptionSetValue(OptionSetMap.ValueType_OptionSet),
                ["dpe_valueoptionsetvalue"] = 1
            };

            ctx.Initialize([rule, condition]);

            var id = Guid.NewGuid();

            var target = new Entity("account", id)
            {
                ["telephone1"] = "555-5555",
                ["statuscode"] = new OptionSetValue(1)
            };

            var preImage = new Entity("account", id)
            {
                ["telephone1"] = "111-1111",
                ["statuscode"] = new OptionSetValue(1)
            };

            var pluginCtx = ctx.GetDefaultPluginContext();
            pluginCtx.MessageName = "Update";
            pluginCtx.InputParameters["Target"] = target;
            pluginCtx.PreEntityImages["PreImage"] = preImage;

            // Should NOT throw
            ctx.ExecutePluginWith<PolicyEnforcementPlugin>(pluginCtx);
        }

        [TestMethod]
        public void Create_Required_Blocks_WhenMissing()
        {
            // Arrange
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
                ["dpe_policyruleid"] = new EntityReference("dpe_policyrule", ruleId),
                ["dpe_sequence"] = 10,
                ["dpe_operator"] = new OptionSetValue(OptionSetMap.Operator_Equals),
                ["dpe_valuetype"] = new OptionSetValue(OptionSetMap.ValueType_OptionSet),
                ["dpe_valueoptionsetvalue"] = 1
            };

            _context.Initialize([rule, condition]);

            var target = new Entity("account", Guid.NewGuid())
            {
                // telephone1 is missing
                ["statuscode"] = new OptionSetValue(1)
            };

            var pluginCtx = _context.GetDefaultPluginContext();
            pluginCtx.MessageName = "Create";
            pluginCtx.InputParameters["Target"] = target;

            // Act + Assert
            Assert.ThrowsException<InvalidPluginExecutionException>(() =>
            {
                _context.ExecutePluginWith<PolicyEnforcementPlugin>(pluginCtx);
            });
        }

        [TestMethod]
        public void Create_Required_Blocks_WhenNull()
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
                ["dpe_policyruleid"] = new EntityReference("dpe_policyrule", ruleId),
                ["dpe_sequence"] = 10,
                ["dpe_operator"] = new OptionSetValue(OptionSetMap.Operator_Equals),
                ["dpe_valuetype"] = new OptionSetValue(OptionSetMap.ValueType_OptionSet),
                ["dpe_valueoptionsetvalue"] = 1
            };

            _context.Initialize([rule, condition]);

            var target = new Entity("account", Guid.NewGuid())
            {
                ["telephone1"] = null,
                ["statuscode"] = new OptionSetValue(1)
            };

            var pluginCtx = _context.GetDefaultPluginContext();
            pluginCtx.MessageName = "Create";
            pluginCtx.InputParameters["Target"] = target;

            Assert.ThrowsException<InvalidPluginExecutionException>(() =>
            {
                _context.ExecutePluginWith<PolicyEnforcementPlugin>(pluginCtx);
            });
        }

        [TestMethod]
        public void Create_Required_Allows_WhenProvided()
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
                ["dpe_policyruleid"] = new EntityReference("dpe_policyrule", ruleId),
                ["dpe_sequence"] = 10,
                ["dpe_operator"] = new OptionSetValue(OptionSetMap.Operator_Equals),
                ["dpe_valuetype"] = new OptionSetValue(OptionSetMap.ValueType_OptionSet),
                ["dpe_valueoptionsetvalue"] = 1
            };

            _context.Initialize([rule, condition]);

            var target = new Entity("account", Guid.NewGuid())
            {
                ["telephone1"] = "555-5555",
                ["statuscode"] = new OptionSetValue(1)
            };

            var pluginCtx = _context.GetDefaultPluginContext();
            pluginCtx.MessageName = "Create";
            pluginCtx.InputParameters["Target"] = target;

            // Should NOT throw
            _context.ExecutePluginWith<PolicyEnforcementPlugin>(pluginCtx);
        }

        [TestMethod]
        public void Create_NotAllowed_Blocks_WhenAttributeIsSet()
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
                ["statecode"] = new OptionSetValue(0)
            };

            var condition = new Entity("dpe_policycondition", Guid.NewGuid())
            {
                ["dpe_policyruleid"] = new EntityReference("dpe_policyrule", ruleId),
                ["dpe_sequence"] = 10,
                ["dpe_operator"] = new OptionSetValue(OptionSetMap.Operator_Equals),
                ["dpe_valuetype"] = new OptionSetValue(OptionSetMap.ValueType_OptionSet),
                ["dpe_valueoptionsetvalue"] = 1
            };

            _context.Initialize([rule, condition]);

            var target = new Entity("account", Guid.NewGuid())
            {
                ["name"] = "Should be blocked",
                ["statuscode"] = new OptionSetValue(1)
            };

            var pluginCtx = _context.GetDefaultPluginContext();
            pluginCtx.MessageName = "Create";
            pluginCtx.InputParameters["Target"] = target;

            Assert.ThrowsException<InvalidPluginExecutionException>(() =>
            {
                _context.ExecutePluginWith<PolicyEnforcementPlugin>(pluginCtx);
            });
        }

        [TestMethod]
        public void Create_NotAllowed_Allows_WhenPolicyDoesNotMatch()
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
                ["statecode"] = new OptionSetValue(0)
            };

            // expects statuscode == 99, but we'll set 1
            var condition = new Entity("dpe_policycondition", Guid.NewGuid())
            {
                ["dpe_policyruleid"] = new EntityReference("dpe_policyrule", ruleId),
                ["dpe_sequence"] = 10,
                ["dpe_operator"] = new OptionSetValue(OptionSetMap.Operator_Equals),
                ["dpe_valuetype"] = new OptionSetValue(OptionSetMap.ValueType_OptionSet),
                ["dpe_valueoptionsetvalue"] = 99
            };

            _context.Initialize([rule, condition]);

            var target = new Entity("account", Guid.NewGuid())
            {
                ["name"] = "Allowed",
                ["statuscode"] = new OptionSetValue(1)
            };

            var pluginCtx = _context.GetDefaultPluginContext();
            pluginCtx.MessageName = "Create";
            pluginCtx.InputParameters["Target"] = target;

            // Should NOT throw
            _context.ExecutePluginWith<PolicyEnforcementPlugin>(pluginCtx);
        }

    }
}
