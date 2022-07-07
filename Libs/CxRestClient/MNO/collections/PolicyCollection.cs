using System;
using System.Collections.Generic;
using System.Text;

namespace CxRestClient.MNO.Collections
{
    public class PolicyCollection
    {
        private Dictionary<int, PolicyDescriptor> _policies = 
            new Dictionary<int, PolicyDescriptor> ();

        private Dictionary<int, PolicyDescriptor> _policiesByRule = 
            new Dictionary<int, PolicyDescriptor>();

        public IEnumerable<PolicyDescriptor> Policies
        {
            get
            {
                return _policies.Values;
            }
        }

        public PolicyDescriptor GetPolicyById (int policyId)
        {
            return _policies[policyId];
        }

        public PolicyDescriptor GetPolicyByRuleId (int ruleId)
        {
            return _policiesByRule[ruleId];
        }

        public void AddPolicy(PolicyDescriptor policy)
        {
            _policies.Add(policy.PolicyId, policy);

            foreach (var rule in policy.Rules.Values)
                _policiesByRule.Add(rule.RuleId, policy);
        }
    }
}
