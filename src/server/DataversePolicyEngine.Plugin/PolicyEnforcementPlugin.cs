using DataversePolicyEngine.Core.Data;
using DataversePolicyEngine.Core.Evaluation;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using System;
using System.Collections.Generic;
using System.Linq;

namespace DataversePolicyEngine.Plugin
{
    public class PolicyEnforcementPlugin : PluginBase
    {
        private const string PreImageName = "PreImage";

        public PolicyEnforcementPlugin()
            : base(typeof(PolicyEnforcementPlugin))
        {
            //Not Implemented
        }

        protected override void ExecuteCdsPlugin(ILocalPluginContext localPluginContext)
        {
            var ctx = localPluginContext.PluginExecutionContext;
            var service = localPluginContext.SystemUserService;
            var tracing = localPluginContext.TracingService;
            if (
                !ctx.InputParameters.Contains("Target")
                || !(ctx.InputParameters["Target"] is Entity target)
            )
                return;

            var message = (ctx.MessageName ?? string.Empty).ToLowerInvariant();
            if (message != "create" && message != "update")
                return;

            // PreImage is required for NotAllowed update comparisons
            Entity preImage = null;
            if (
                message == "update"
                && ctx.PreEntityImages != null
                && ctx.PreEntityImages.Contains(PreImageName)
            )
                preImage = ctx.PreEntityImages[PreImageName];

            var entityLogicalName = target.LogicalName;

            // Discover which attributes have policies for this entity
            // (so we don't evaluate every attribute in Target blindly)
            var governedAttributes = GetGovernedAttributes(service, entityLogicalName);

            var repo = new PolicyRepository();
            var evaluator = new PolicyEvaluator(repo);

            foreach (var attr in governedAttributes)
            {
                // If it's update, we only care if the attribute is being changed OR required could be violated by nulling
                // For create, we check even if missing (required-on-create scenario)
                var isCreate = message == "create";
                var isUpdate = message == "update";

                var inTarget = target.Attributes.ContainsKey(attr);

                // ---------
                // NotAllowed enforcement (Update only, change detection)
                // ---------
                if (isUpdate && inTarget)
                {
                    var decision = evaluator.EvaluateAttribute(
                        service,
                        entityLogicalName,
                        attr,
                        target,
                        preImage
                    );

                    if (decision.NotAllowed)
                    {
                        var newValue = target[attr];
                        var oldValue =
                            (preImage != null && preImage.Attributes.ContainsKey(attr))
                                ? preImage[attr]
                                : null;

                        if (!ValueEquality.AreEqual(newValue, oldValue))
                        {
                            throw new InvalidPluginExecutionException(
                                $"Change blocked by policy: {entityLogicalName}.{attr} is not allowed to change."
                            );
                        }
                    }
                }

                // ---------
                // Required enforcement
                // ---------
                // Update: if user is setting the field to null, block if required
                if (isUpdate && inTarget)
                {
                    var decision = evaluator.EvaluateAttribute(
                        service,
                        entityLogicalName,
                        attr,
                        target,
                        preImage
                    );

                    if (decision.Required)
                    {
                        var newValue = target[attr];
                        if (newValue == null)
                        {
                            throw new InvalidPluginExecutionException(
                                $"Policy violation: {entityLogicalName}.{attr} is required."
                            );
                        }
                    }
                }

                // Create: required means must exist and be non-null
                if (isCreate)
                {
                    // Build a "current" view: on create, Target is the row; no preimage.
                    // If missing -> treat as null.
                    var decision = evaluator.EvaluateAttribute(
                        service,
                        entityLogicalName,
                        attr,
                        target,
                        null
                    );

                    if (decision.Required)
                    {
                        if (!target.Attributes.ContainsKey(attr) || target[attr] == null)
                        {
                            throw new InvalidPluginExecutionException(
                                $"Policy violation: {entityLogicalName}.{attr} is required."
                            );
                        }
                    }

                    // NotAllowed on create: if present and non-null, block (deny set)
                    if (
                        decision.NotAllowed
                        && target.Attributes.ContainsKey(attr)
                        && target[attr] != null
                    )
                    {
                        throw new InvalidPluginExecutionException(
                            $"Change blocked by policy: {entityLogicalName}.{attr} is not allowed to be set."
                        );
                    }
                }
            }
        }

        private static HashSet<string> GetGovernedAttributes(
            IOrganizationService service,
            string entityLogicalName
        )
        {
            // Active = statecode == 0 (you switched to this)
            var q = new QueryExpression("dpe_policyrule")
            {
                ColumnSet = new ColumnSet("dpe_targetattributelogicalname"),
                Distinct = true
            };

            q.Criteria.AddCondition(
                "dpe_targetentitylogicalname",
                ConditionOperator.Equal,
                entityLogicalName
            );
            q.Criteria.AddCondition("statecode", ConditionOperator.Equal, 0);

            // optional: only include the 3 types we care about (not strictly necessary)
            // q.Criteria.AddCondition("dpe_policytype", ConditionOperator.In,
            //    OptionSetMap.PolicyType_Visible, OptionSetMap.PolicyType_Required, OptionSetMap.PolicyType_NotAllowed);

            var results = service.RetrieveMultiple(q).Entities;

            return new HashSet<string>(
                results
                    .Select(e => e.GetAttributeValue<string>("dpe_targetattributelogicalname"))
                    .Where(s => !string.IsNullOrWhiteSpace(s)),
                StringComparer.OrdinalIgnoreCase
            );
        }
    }
}
