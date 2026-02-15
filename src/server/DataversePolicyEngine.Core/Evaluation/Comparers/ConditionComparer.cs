using DataversePolicyEngine.Core.Model;
using Microsoft.Xrm.Sdk;

namespace DataversePolicyEngine.Core.Evaluation.Comparers
{
    public static class ConditionComparer
    {
        public static bool Matches(
            Entity condition,
            string triggerAttributeLogicalName,
            Entity target,
            Entity preImage
        )
        {
            object currentValue = null;

            if (target != null && target.Attributes.ContainsKey(triggerAttributeLogicalName))
                currentValue = target[triggerAttributeLogicalName];
            else if (
                preImage != null && preImage.Attributes.ContainsKey(triggerAttributeLogicalName)
            )
                currentValue = preImage[triggerAttributeLogicalName];

            var opOs = condition.GetAttributeValue<OptionSetValue>("dpe_operator");
            if (opOs == null)
                return false;

            OperatorType op = OptionSetMap.OperatorFromOptionValue(opOs.Value);

            switch (op)
            {
                case OperatorType.Equals:
                    return DataverseValueComparer.Equals(condition, currentValue);

                case OperatorType.NotEquals:
                    return !DataverseValueComparer.Equals(condition, currentValue);

                case OperatorType.IsNull:
                    return currentValue == null;

                case OperatorType.IsNotNull:
                    return currentValue != null;

                default:
                    return false;
            }
        }
    }
}
