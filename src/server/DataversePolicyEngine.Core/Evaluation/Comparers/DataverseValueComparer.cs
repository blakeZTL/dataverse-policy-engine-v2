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
                case 100000000: // String
                    return string.Equals(
                        currentValue.ToString(),
                        condition.GetAttributeValue<string>("dpe_valuestring"),
                        System.StringComparison.OrdinalIgnoreCase
                    );

                case 100000001: // Number
                    return System.Convert.ToDecimal(currentValue)
                        == condition.GetAttributeValue<decimal>("dpe_valuenumber");

                case 100000002: // Boolean
                    return (bool)currentValue == condition.GetAttributeValue<bool>("dpe_valueboolean");

                case 100000003: // OptionSet
                    return ((OptionSetValue)currentValue).Value
                        == condition.GetAttributeValue<int>("dpe_valueoptionsetvalue");

                case 100000004: // Lookup
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
