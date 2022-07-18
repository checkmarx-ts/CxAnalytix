using System;
using System.Collections.Generic;
using System.Text;

namespace CxRestClient.MNO.Collections
{
    public abstract class PolicyRuleDescriptor
    {
        public PolicyRuleDescriptor()
        {
        }

        public abstract int PolicyId { get; set; }
        public int RuleId { get; set; }
        public String Name { get; set; }
        public String Description { get; set; }
        public String ScanProduct { get; set; }
        public String RuleType { get; set; }
        public DateTime CreatedOn { get; set; }
    }
}
