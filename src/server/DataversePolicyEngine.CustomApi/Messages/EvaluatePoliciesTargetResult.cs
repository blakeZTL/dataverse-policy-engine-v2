using System.Runtime.Serialization;

namespace DataversePolicyEngine.CustomApi.Messages
{
    [DataContract]
    public class EvaluatePoliciesTargetResult
    {
        [DataMember]
        public string Target { get; set; }

        [DataMember]
        public bool Visible { get; set; }

        [DataMember]
        public bool Required { get; set; }

        [DataMember]
        public bool NotAllowed { get; set; }
    }
}
