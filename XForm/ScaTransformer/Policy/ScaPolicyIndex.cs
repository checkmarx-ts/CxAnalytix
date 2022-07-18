using CxRestClient.SCA;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CxAnalytix.XForm.ScaTransformer.Policy
{
    internal class ScaPolicyIndex
    {

        private HashSet<CxPolicies.Policy> _global = new();
        private Dictionary<String, HashSet<CxPolicies.Policy> > _specific = new();

        public ScaPolicyIndex() { }

        public ScaPolicyIndex(IEnumerable<CxPolicies.Policy> policies)
        {
            AddPolicy(policies);
        }

        public void AddPolicy(CxPolicies.Policy policy)
        {
            if (policy.Global)
                _global.Add(policy);
            else
            {
                foreach (var projectId in policy.Projects)
                {
                    if (!_specific.ContainsKey(projectId))
                        _specific.Add(projectId, new());

                    _specific[projectId].Add(policy);
                }
            }
        }

        public void AddPolicy(IEnumerable<CxPolicies.Policy> policies)
        {
            foreach(var policy in policies)
                AddPolicy(policy); 
        }

        public IEnumerable<CxPolicies.Policy> GetPoliciesForProject(String projectId)
        {
            LinkedList<CxPolicies.Policy> retVal = new(_global);
            if (_specific.ContainsKey(projectId))
                return retVal.Concat(_specific[projectId]);
            else
                return retVal;
        }
    }
}
