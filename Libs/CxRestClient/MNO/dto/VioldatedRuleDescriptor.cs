using SDK.Modules.Transformer.Data;
using System;
using System.Collections.Generic;
using System.Text;

namespace CxRestClient.MNO.dto
{
    public class ViolatedRuleDescriptor : MNORuleDescriptor
    {
        internal ViolatedRuleDescriptor ()
        { }

        public ViolatedRuleDescriptor (PolicyRuleDescriptor src)
        {
            this.CreatedOn = src.CreatedOn;
            this.Description = src.Description;
            this.Name = src.Name;
            this.RuleId = src.RuleId;
            this.RuleType = src.RuleType;
            this.ScanProduct = src.ScanProduct;
        }

        public int ProjectId { get; internal set; }

        public DateTime FirstDetectionDate { get; internal set; }
        public String ScanId { get; internal set; }
        public String ViolationType { get; internal set; }
        public String ViolationName { get; internal set; }
        public String ViolationSource { get; internal set; }
        public String ViolationSeverity { get; internal set; }
        public DateTime? ViolationOccured { get; internal set; }
        public double? ViolationRiskScore { get; internal set; }
        public String ViolationStatus { get; internal set; }
        public String ViolationState { get; internal set; }
        public String ViolationId { get; internal set; }

        public override string ToString()
        {
            return $"CreatedOn: {CreatedOn} PolicyId: {PolicyId} ProjectId: {ProjectId} " +
                $"ScanId: {ScanId} RuleId: {RuleId}";
        }
    }
}
