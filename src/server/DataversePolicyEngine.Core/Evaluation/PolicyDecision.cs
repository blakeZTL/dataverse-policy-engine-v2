namespace DataversePolicyEngine.Core.Evaluation
{
    public sealed class PolicyDecision
    {
        public bool Visible { get; set; } = true;
        public bool Required { get; set; } = false;
        public bool NotAllowed { get; set; } = false;

        public string VisibleMatchedRuleId { get; set; }
        public string RequiredMatchedRuleId { get; set; }
        public string NotAllowedMatchedRuleId { get; set; }
    }
}
