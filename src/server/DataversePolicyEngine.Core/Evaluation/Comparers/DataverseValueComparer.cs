using Microsoft.Xrm.Sdk;
using System;

namespace DataversePolicyEngine.Core.Evaluation.Comparers
{
    public static class DataverseValueComparer
    {
        public static bool Equals(Entity condition, object currentValue)
        {
            if (currentValue == null)
                return false;

            var valueType = condition.GetAttributeValue<OptionSetValue>("dpe_valuetype")?.Value;

            switch (valueType)
            {
                case 0: // String
                    return string.Equals(
                        currentValue.ToString(),
                        condition.GetAttributeValue<string>("dpe_valuestring"),
                        System.StringComparison.OrdinalIgnoreCase
                    );

                case 1: // Number
                    return System.Convert.ToDecimal(currentValue)
                        == condition.GetAttributeValue<decimal>("dpe_valuenumber");

                case 5: // Boolean
                    return (bool)currentValue == condition.GetAttributeValue<bool>("dpe_valueboolean");

                case 6: // OptionSet
                    return ((OptionSetValue)currentValue).Value
                        == condition.GetAttributeValue<int>("dpe_valueoptionsetvalue");

                case 7: // Lookup
                    var er = (EntityReference)currentValue;

                    return er.Id == condition.GetAttributeValue<Guid>("dpe_valuelookupid")
                        && er.LogicalName
                            == condition.GetAttributeValue<string>("dpe_valuelookuplogicalname");

                default:
                    return false;
            }
        }
    }
}
