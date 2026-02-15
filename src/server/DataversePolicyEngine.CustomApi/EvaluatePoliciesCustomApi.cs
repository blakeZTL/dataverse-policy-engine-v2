using DataversePolicyEngine.Core.Data;
using DataversePolicyEngine.Core.Evaluation;
using DataversePolicyEngine.CustomApi.Messages;
using Microsoft.Xrm.Sdk;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;

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
            // Accept either the new batched param OR the legacy single-target param
            var targetAttributeLogicalNames = GetOptionalStringArray(
                ctx,
                "TargetAttributeLogicalNames"
            );

            if (targetAttributeLogicalNames == null || targetAttributeLogicalNames.Length == 0)
            {
                // legacy fallback
                var single = GetRequired<string>(ctx, "TargetAttributeLogicalName");
                targetAttributeLogicalNames = new[] { single };
            }
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
            var response = new EvaluatePoliciesBatchResponse();

            foreach (var targetAttr in targetAttributeLogicalNames)
            {
                var decision = evaluator.EvaluateAttribute(
                    service,
                    entityLogicalName,
                    targetAttr,
                    target,
                    preImage: null
                );

                response.Results.Add(
                    new EvaluatePoliciesTargetResult
                    {
                        Target = targetAttr,
                        Visible = decision.Visible,
                        Required = decision.Required,
                        NotAllowed = decision.NotAllowed
                    }
                );
            }

            // New output (preferred)
            ctx.OutputParameters["ResultsJson"] = JsonSerializer.Serialize(response);

            // Legacy outputs (optional/backward-compatible)
            if (targetAttributeLogicalNames.Length == 1 && response.Results.Count == 1)
            {
                var first = response.Results[0];
                ctx.OutputParameters["Visible"] = first.Visible;
                ctx.OutputParameters["Required"] = first.Required;
                ctx.OutputParameters["NotAllowed"] = first.NotAllowed;
            }
        }

        private static string[] GetOptionalStringArray(IPluginExecutionContext ctx, string name)
        {
            if (!ctx.InputParameters.Contains(name) || ctx.InputParameters[name] == null)
                return null;

            var raw = ctx.InputParameters[name];

            // Most common case
            if (raw is string[] sArr)
                return sArr;

            // Sometimes it arrives as object[]
            if (raw is object[] oArr)
                return oArr.OfType<string>().ToArray();

            // Sometimes as IEnumerable<string>
            if (raw is IEnumerable<string> e)
                return e.ToArray();

            // Unexpected type
            throw new InvalidPluginExecutionException(
                $"Parameter '{name}' was not a string array. Actual type: {raw.GetType().FullName}"
            );
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
