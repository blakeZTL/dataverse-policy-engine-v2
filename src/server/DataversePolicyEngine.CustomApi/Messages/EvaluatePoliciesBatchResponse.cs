using System.Collections.Generic;
using System.Runtime.Serialization;

namespace DataversePolicyEngine.CustomApi.Messages
{
    [DataContract]
    public class EvaluatePoliciesBatchResponse
    {
        [DataMember]
        public List<EvaluatePoliciesTargetResult> Results { get; set; } =
            new List<EvaluatePoliciesTargetResult>();
    }
}
