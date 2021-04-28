using System;
using CxRestClient.MNO.dto;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace CxAnalytix.TransformLogic.Data
{
    class ProjectPolicyIndex : PolicyCollection
    {

        public ProjectPolicyIndex(PolicyCollection src)
        {
            if (src == null)
                return;

            foreach (var p in src.Policies)
                AddPolicy(p);
        }

        private ConcurrentDictionary<int, LinkedList<PolicyDescriptor>> _projectPolicies = 
            new ConcurrentDictionary<int, LinkedList<PolicyDescriptor>>();


		public void CorrelateProjectToPolicies(int projectId, IEnumerable<int> policies)
		{
			if (policies == null)
				return;

			if (_projectPolicies.TryAdd(projectId, new LinkedList<PolicyDescriptor>()))
				foreach (var policyId in policies)
				{
					_projectPolicies[projectId].AddLast(this.GetPolicyById(policyId));
				}

		}

		public IEnumerable<PolicyDescriptor> GetPoliciesForProject (int projectId)
        {
            return _projectPolicies[projectId];
        }


    }
}
