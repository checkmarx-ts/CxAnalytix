using System;
using System.Collections.Generic;
using System.Text;

namespace CxRestClient.MNO
{
    public class ViolatedPolicyCollection
    {
        private Dictionary<String, LinkedList<ViolatedRuleDescriptor>> _violatedRulesByScanId =
            new Dictionary<string, LinkedList<ViolatedRuleDescriptor>>();

        public IEnumerable<ViolatedRuleDescriptor> GetViolatedRulesByScanId (String scanId)
        {
            if (_violatedRulesByScanId.ContainsKey(scanId))
                return _violatedRulesByScanId[scanId];
            else
                return null;
        }

        public Dictionary<String, LinkedList<ViolatedRuleDescriptor>> Rules { get => _violatedRulesByScanId;
        }

        public void AddViolatedRule(ViolatedRuleDescriptor rule)
        {
            if (!_violatedRulesByScanId.ContainsKey(rule.ScanId))
                _violatedRulesByScanId.Add(rule.ScanId, new LinkedList<ViolatedRuleDescriptor>());

            _violatedRulesByScanId[rule.ScanId].AddLast(rule);
        }
    }
}
