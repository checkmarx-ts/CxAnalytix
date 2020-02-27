using CxRestClient;
using System;
using System.Collections.Generic;
using System.Text;

namespace CxAnalytix.TransformLogic.Data
{
    class ProjectPolicyIndex : PolicyCollection
    {

        public ProjectPolicyIndex(PolicyCollection src)
        {
            foreach (var p in src.Policies)
                AddPolicy(p);
        }

        private Dictionary<int, LinkedList<PolicyDescriptor>> _projectPolicies = 
            new Dictionary<int, LinkedList<PolicyDescriptor>>();
        

        public void CorrelateProjectToPolicies (int projectId, IEnumerable<int> policies)
        {
            if (!_projectPolicies.ContainsKey(projectId))
                _projectPolicies.Add(projectId, new LinkedList<PolicyDescriptor> () );

            foreach (var policyId in policies)
            {
                _projectPolicies[projectId].AddLast(this.GetPolicyById (policyId) );
            }

        }

        public IEnumerable<PolicyDescriptor> GetPoliciesForProject (int projectId)
        {
            return _projectPolicies[projectId];
        }


    }
}
