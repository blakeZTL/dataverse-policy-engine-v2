using DataversePolicyEngine.Core.Data;
using Microsoft.Xrm.Sdk;

namespace DataversePolicyEngine.Tests.Core.Data
{
    [TestClass]
    public class PolicyRepositoryTests : FakeXrmEasyTestBase
    {
        [TestMethod]
        public void GetRules_FiltersByEntityAttributeActive_AndOrdersBySequence()
        {
            var ctx = _context;
            var svc = ctx.GetOrganizationService();

            // matching rules (account.name, active)
            var r1 = new Entity("dpe_policyrule", Guid.NewGuid())
            {
                ["dpe_targetentitylogicalname"] = "account",
                ["dpe_targetattributelogicalname"] = "name",
                ["statecode"] = new OptionSetValue(0),
                ["dpe_sequence"] = 20
            };

            var r2 = new Entity("dpe_policyrule", Guid.NewGuid())
            {
                ["dpe_targetentitylogicalname"] = "account",
                ["dpe_targetattributelogicalname"] = "name",
                ["statecode"] = new OptionSetValue(0),
                ["dpe_sequence"] = 10
            };

            // non-matching (inactive)
            var r3 = new Entity("dpe_policyrule", Guid.NewGuid())
            {
                ["dpe_targetentitylogicalname"] = "account",
                ["dpe_targetattributelogicalname"] = "name",
                ["statecode"] = new OptionSetValue(1),
                ["dpe_sequence"] = 5
            };

            // non-matching (different attribute)
            var r4 = new Entity("dpe_policyrule", Guid.NewGuid())
            {
                ["dpe_targetentitylogicalname"] = "account",
                ["dpe_targetattributelogicalname"] = "telephone1",
                ["statecode"] = new OptionSetValue(0),
                ["dpe_sequence"] = 1
            };

            // non-matching (different entity)
            var r5 = new Entity("dpe_policyrule", Guid.NewGuid())
            {
                ["dpe_targetentitylogicalname"] = "contact",
                ["dpe_targetattributelogicalname"] = "name",
                ["statecode"] = new OptionSetValue(0),
                ["dpe_sequence"] = 1
            };

            ctx.Initialize([r1, r2, r3, r4, r5]);

            var repo = new PolicyRepository();

            var results = repo.GetRules(svc, "account", "name");

            Assert.HasCount(
                2,
                results,
                "Should return only active rules matching entity+attribute."
            );
            Assert.AreEqual(r2.Id, results[0].Id, "Should be ordered by sequence ascending.");
            Assert.AreEqual(r1.Id, results[1].Id, "Should be ordered by sequence ascending.");
        }

        [TestMethod]
        public void GetConditions_FiltersByRule_AndOrdersBySequence()
        {
            var ctx = _context;
            var svc = ctx.GetOrganizationService();

            var ruleId = Guid.NewGuid();

            var c1 = new Entity("dpe_policycondition", Guid.NewGuid())
            {
                ["dpe_policyrule"] = new EntityReference("dpe_policyrule", ruleId),
                ["dpe_sequence"] = 20
            };

            var c2 = new Entity("dpe_policycondition", Guid.NewGuid())
            {
                ["dpe_policyrule"] = new EntityReference("dpe_policyrule", ruleId),
                ["dpe_sequence"] = 10
            };

            // non-matching (different rule)
            var c3 = new Entity("dpe_policycondition", Guid.NewGuid())
            {
                ["dpe_policyrule"] = new EntityReference("dpe_policyrule", Guid.NewGuid()),
                ["dpe_sequence"] = 1
            };

            ctx.Initialize(new[] { c1, c2, c3 });

            var repo = new PolicyRepository();

            var results = repo.GetConditions(svc, ruleId);

            Assert.HasCount(2, results, "Should return only conditions for the specified rule.");
            Assert.AreEqual(c2.Id, results[0].Id, "Should be ordered by sequence ascending.");
            Assert.AreEqual(c1.Id, results[1].Id, "Should be ordered by sequence ascending.");
        }
    }
}
