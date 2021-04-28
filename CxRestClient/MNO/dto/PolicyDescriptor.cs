using System;
using System.Collections.Generic;

namespace CxRestClient.MNO.dto
{
    public class PolicyDescriptor
    {
        internal PolicyDescriptor ()
        {
        }

        public PolicyDescriptor(PolicyDescriptor src)
        {
            this.PolicyId = src.PolicyId;
            this.Name = src.Name;
            this.Description = src.Description;
            this.isActive = src.isActive;
            this.CreatedOn = src.CreatedOn;
        }


        public int PolicyId { get; set; }
        public String Name { get; set; }
        public String Description { get; set; }
        public Boolean isActive { get; set; }
        public DateTime CreatedOn { get; set; }


        private Dictionary<int, RuleDescriptor> _rules = new Dictionary<int, RuleDescriptor>();
        public IReadOnlyDictionary<int, RuleDescriptor> Rules
        {
            get => _rules;
        }
        

        public void AddRule (RuleDescriptor rule)
        {
            _rules.Add(rule.RuleId, rule);
        }


        public void AddRule(IEnumerable<RuleDescriptor> rules)
        {
            foreach (var rule in rules)
                AddRule(rule);
        }
    }
}
