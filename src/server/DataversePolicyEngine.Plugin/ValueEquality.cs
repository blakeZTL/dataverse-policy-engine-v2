using Microsoft.Xrm.Sdk;
using System;

namespace DataversePolicyEngine.Plugin
{
    internal static class ValueEquality
    {
        public static bool AreEqual(object a, object b)
        {
            if (ReferenceEquals(a, b))
                return true;
            if (a == null || b == null)
                return false;

            // Unwrap AliasedValue
            if (a is AliasedValue avA)
                a = avA.Value;
            if (b is AliasedValue avB)
                b = avB.Value;

            if (a is OptionSetValue osA && b is OptionSetValue osB)
                return osA.Value == osB.Value;

            if (a is EntityReference erA && b is EntityReference erB)
                return erA.Id == erB.Id
                    && string.Equals(
                        erA.LogicalName,
                        erB.LogicalName,
                        StringComparison.OrdinalIgnoreCase
                    );

            if (a is Money mA && b is Money mB)
                return mA.Value == mB.Value;

            if (a is DateTime dtA && b is DateTime dtB)
                return dtA == dtB;

            // Most primitives/strings
            return a.Equals(b);
        }
    }
}
