using DataversePolicyEngine.Core.Data;
using DataversePolicyEngine.Core.Evaluation;
using Microsoft.Xrm.Sdk;
using System;

namespace DataversePolicyEngine.CustomApi
{
    public class EvaluatePoliciesCustomApi : PluginBase
    {
        public EvaluatePoliciesCustomApi()
            : base(typeof(EvaluatePoliciesCustomApi))
        {
            // Not implemented
        }

        protected override void ExecuteCdsPlugin(ILocalPluginContext localPluginContext)
        {
            var ctx = localPluginContext.PluginExecutionContext;
            var service = localPluginContext.SystemUserService;
            var tracing = localPluginContext.TracingService;
            // Inputs
            var entityLogicalName = GetRequired<string>(ctx, "EntityLogicalName");
            var targetAttributeLogicalName = GetRequired<string>(ctx, "TargetAttributeLogicalName");
            var triggerAttributeLogicalName = GetRequired<string>(
                ctx,
                "TriggerAttributeLogicalName"
            );

            // Build a "target" entity containing the trigger attribute value
            var target = new Entity(entityLogicalName);

            var triggerValue = ResolveTriggerValue(ctx);
            if (triggerValue.isProvided)
            {
                target[triggerAttributeLogicalName] = triggerValue.value;
            }

            var evaluator = new PolicyEvaluator(new PolicyRepository());
            var decision = evaluator.EvaluateAttribute(
                service,
                entityLogicalName,
                targetAttributeLogicalName,
                target,
                preImage: null
            );

            // Outputs
            ctx.OutputParameters["Visible"] = decision.Visible;
            ctx.OutputParameters["Required"] = decision.Required;
            ctx.OutputParameters["NotAllowed"] = decision.NotAllowed;
        }

        private static (bool isProvided, object value) ResolveTriggerValue(
            IPluginExecutionContext ctx
        )
        {
            // Priority order: Lookup, OptionSet, Boolean, Number, String (pick one)
            var lookupLogicalName = GetOptional<string>(ctx, "TriggerLookupLogicalName");
            var lookupId = GetOptional<Guid?>(ctx, "TriggerLookupId");

            if (
                !string.IsNullOrWhiteSpace(lookupLogicalName)
                && lookupId.HasValue
                && lookupId.Value != Guid.Empty
            )
            {
                return (true, new EntityReference(lookupLogicalName, lookupId.Value));
            }

            var os = GetOptional<int?>(ctx, "TriggerOptionSetValue");
            if (os.HasValue)
            {
                return (true, new OptionSetValue(os.Value));
            }

            var b = GetOptional<bool?>(ctx, "TriggerBoolean");
            if (b.HasValue)
            {
                return (true, b.Value);
            }

            var n = GetOptional<decimal?>(ctx, "TriggerNumber");
            if (n.HasValue)
            {
                return (true, n.Value);
            }

            var s = GetOptional<string>(ctx, "TriggerString");
            if (s != null)
            {
                return (true, s);
            }

            // No trigger provided (allowed for IsNull/IsNotNull scenarios)
            return (false, null);
        }

        private static T GetRequired<T>(IPluginExecutionContext ctx, string name)
        {
            if (!ctx.InputParameters.Contains(name) || ctx.InputParameters[name] == null)
                throw new InvalidPluginExecutionException($"Missing required parameter: {name}");

            return (T)ctx.InputParameters[name];
        }

        private static T GetOptional<T>(IPluginExecutionContext ctx, string name)
        {
            if (!ctx.InputParameters.Contains(name) || ctx.InputParameters[name] == null)
                return default(T);

            return (T)ctx.InputParameters[name];
        }
    }
}
