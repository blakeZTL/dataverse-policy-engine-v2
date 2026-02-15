using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataversePolicyEngine.Core.Model
{
    public static class OptionSetMap
    {
        // TODO: replace these with your actual Dataverse option values
        public const int PolicyType_Visible = (int)PolicyType.Visible;
        public const int PolicyType_Required = (int)PolicyType.Required;
        public const int PolicyType_NotAllowed = (int)PolicyType.NotAllowed;

        public const int Operator_Equals = (int)OperatorType.Equals;
        public const int Operator_NotEquals = (int)OperatorType.NotEquals;
        public const int Operator_IsNull = (int)OperatorType.IsNull;
        public const int Operator_IsNotNull = (int)OperatorType.IsNotNull;

        public const int ValueType_String = (int)ValueType.String;
        public const int ValueType_Number = (int)ValueType.Number;
        public const int ValueType_Boolean = (int)ValueType.Boolean;
        public const int ValueType_OptionSet = (int)ValueType.OptionSet;
        public const int ValueType_Lookup = (int)ValueType.Lookup;

        public static int ToOptionValue(PolicyType type)
        {
            switch (type)
            {
                case PolicyType.Visible:
                    return PolicyType_Visible;
                case PolicyType.Required:
                    return PolicyType_Required;
                case PolicyType.NotAllowed:
                    return PolicyType_NotAllowed;
                default:
                    throw new ArgumentOutOfRangeException(nameof(type));
            }
        }

        public static OperatorType OperatorFromOptionValue(int optionValue)
        {
            switch (optionValue)
            {
                case Operator_Equals:
                    return OperatorType.Equals;
                case Operator_NotEquals:
                    return OperatorType.NotEquals;
                case Operator_IsNull:
                    return OperatorType.IsNull;
                case Operator_IsNotNull:
                    return OperatorType.IsNotNull;
                default:
                    throw new ArgumentOutOfRangeException(
                        nameof(optionValue),
                        "Unknown operator option value."
                    );
            }
        }

        public static ValueType ValueTypeFromOptionValue(int optionValue)
        {
            switch (optionValue)
            {
                case ValueType_String:
                    return ValueType.String;
                case ValueType_Number:
                    return ValueType.Number;
                case ValueType_Boolean:
                    return ValueType.Boolean;
                case ValueType_OptionSet:
                    return ValueType.OptionSet;
                case ValueType_Lookup:
                    return ValueType.Lookup;
                default:
                    throw new ArgumentOutOfRangeException(
                        nameof(optionValue),
                        "Unknown value type option value."
                    );
            }
        }
    }
}
