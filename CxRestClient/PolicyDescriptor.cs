using System;
using System.Collections.Generic;
using System.Text;

namespace CxRestClient
{
    public class PolicyDescriptor
    {
        internal PolicyDescriptor ()
        {
            Rules = new LinkedList<RuleDescriptor>();
        }

        public int PolicyId { get; set; }
        public String Name {get; set;}
        public String Description { get; set; }
        public Boolean isActive { get; set; }
        public DateTime CreatedOn { get; set; }

        public LinkedList<RuleDescriptor> Rules { get; internal set; }

        public void AddRule (RuleDescriptor rule)
        {
            Rules.AddLast(rule);
        }


        public void AddRule(IEnumerable<RuleDescriptor> rules)
        {
            foreach (var rule in rules)
                AddRule(rule);
        }
    }
}
