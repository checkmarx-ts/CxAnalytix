using System;
using System.Collections.Generic;

namespace SDK.Modules.Transformer.Data
{
    public class PolicyDescriptor
    {
        public PolicyDescriptor ()
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
        public String? Name { get; set; }
        public String? Description { get; set; }
        public Boolean isActive { get; set; }
        public DateTime CreatedOn { get; set; }


        private Dictionary<int, PolicyRuleDescriptor> _rules = new Dictionary<int, PolicyRuleDescriptor>();
        public IReadOnlyDictionary<int, PolicyRuleDescriptor> Rules
        {
            get => _rules;
        }
        

        public void AddRule (PolicyRuleDescriptor rule)
        {
            _rules.Add(rule.RuleId, rule);
        }


        public void AddRule(IEnumerable<PolicyRuleDescriptor> rules)
        {
            foreach (var rule in rules)
                AddRule(rule);
        }
    }
}
