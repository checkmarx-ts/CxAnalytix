using log4net;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Threading;
using CxRestClient.Utility;
using CxRestClient.MNO.dto;

namespace CxRestClient.MNO
{
    public class CxMnoPolicies
    {
        private static ILog _log = LogManager.GetLogger(typeof(CxMnoPolicies));
        private static String PROJECT_POLICY_URL_SUFFIX =
            "cxarm/policymanager/projects/{0}/policies";

        private static String POLICY_LIST_URL_SUFFIX =
            "cxarm/policymanager/policies";

        private CxMnoPolicies()
        { }


        private static String GetFlatPolicyNames(JToken payload)
        {
            using (var reader = new JTokenReader(payload))
            {
                StringBuilder b = new StringBuilder();

                while (JsonUtils.MoveToNextProperty(reader, "name"))
                {
                    var policyName = ((JProperty)reader.CurrentToken).Value.ToString();


                    if (JsonUtils.MoveToNextProperty(reader, "isActive"))
                    {
                        if (Boolean.Parse(((JProperty)reader.CurrentToken).Value.ToString()))
                        {
                            if (b.Length > 0)
                                b.Append(';');

                            b.Append(policyName);
                        }
                    }
                    else
                        throw new InvalidDataException("Expected key 'isActive' but did not find it.");
                }

                return b.ToString();
            }
        }

        public static String GetProjectPoliciesSingleField(CxRestContext ctx,
                CancellationToken token, int projectId)
        {
            return WebOperation.ExecuteGet<String>(
                ctx.Json.CreateMnoClient
                , (response) =>
                {
                    JToken jt = JToken.Load(new JsonTextReader(new StreamReader
                        (response.Content.ReadAsStreamAsync().Result)));

                    return GetFlatPolicyNames(jt);
                }
                , CxRestContext.MakeUrl(ctx.MnoUrl, String.Format(PROJECT_POLICY_URL_SUFFIX, projectId))
                , ctx
                , token, apiVersion: null);
        }

        private static PolicyCollection ParsePolicies(CxRestContext ctx,
                CancellationToken token, JToken policyPayload)
        {

            PolicyCollection result = new PolicyCollection();

            using (JTokenReader reader = new JTokenReader(policyPayload))
                while (JsonUtils.MoveToNextProperty(reader, "id"))
                {
                    PolicyDescriptor policy = new PolicyDescriptor()
                    {
                        PolicyId = Convert.ToInt32(((JProperty)reader.CurrentToken).Value)
                    };

                    if (!JsonUtils.MoveToNextProperty(reader, "name"))
                        continue;
                    policy.Name = ((JProperty)reader.CurrentToken).Value.ToString();

                    if (!JsonUtils.MoveToNextProperty(reader, "description"))
                        continue;
                    policy.Description = ((JProperty)reader.CurrentToken).Value.ToString();

                    if (!JsonUtils.MoveToNextProperty(reader, "isActive"))
                        continue;
                    policy.isActive = Convert.ToBoolean(((JProperty)reader.CurrentToken).Value);

                    if (!JsonUtils.MoveToNextProperty(reader, "createdOn"))
                        continue;
                    policy.CreatedOn = JsonUtils.UtcEpochTimeToDateTime
                        (Convert.ToInt64(((JProperty)reader.CurrentToken).Value) / 1000);

                    var rules = CxMnoPolicyRules.GetRulesForPolicy(ctx, token, policy.PolicyId);
                    policy.AddRule(rules);

                    result.AddPolicy(policy);
                }

            return result;
        }

		public static PolicyCollection GetAllPolicies(CxRestContext ctx,
				CancellationToken token)
		{
			return WebOperation.ExecuteGet<PolicyCollection>(
				ctx.Json.CreateMnoClient
				, (response) =>
				{
					JToken jt = JToken.Load(new JsonTextReader(new StreamReader
						(response.Content.ReadAsStreamAsync().Result)));

					return ParsePolicies(ctx, token, jt);
				}
				, CxRestContext.MakeUrl(ctx.MnoUrl, POLICY_LIST_URL_SUFFIX)
				, ctx
				, token
				, exceptionErrorLogic: (ex) =>
				{

                    if (ex is System.AggregateException)
                        foreach (var x in (ex as System.AggregateException).InnerExceptions)
                            if (x is System.Net.Http.HttpRequestException)
                                return false;

					return true;
				}, apiVersion: null);
		}


		public static IEnumerable<int> GetPolicyIdsForProject(CxRestContext ctx,
                CancellationToken token, int projectId)
        {
			return WebOperation.ExecuteGet<IEnumerable<int>>(
			ctx.Json.CreateMnoClient
			, (response) =>
			{
				JToken jt = JToken.Load(new JsonTextReader(new StreamReader
	                (response.Content.ReadAsStreamAsync().Result)));

				LinkedList<int> policyIds = new LinkedList<int>();

				using (JTokenReader reader = new JTokenReader(jt))
					while (JsonUtils.MoveToNextProperty(reader, "id"))
					{
						policyIds.AddLast(Convert.ToInt32(((JProperty)reader.CurrentToken).Value));
					}

				return policyIds;

			}
			, CxRestContext.MakeUrl(ctx.MnoUrl, String.Format(PROJECT_POLICY_URL_SUFFIX, projectId))
			, ctx
			, token, apiVersion: null);
		}
	}
}
