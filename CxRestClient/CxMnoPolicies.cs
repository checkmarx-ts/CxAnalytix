using log4net;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;

namespace CxRestClient
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
            using (var client = ctx.Json.CreateMnoClient())
            {

                using (var policyPayload = client.GetAsync(CxRestContext.MakeUrl(ctx.MnoUrl,
                    String.Format(PROJECT_POLICY_URL_SUFFIX, projectId)), token).Result)
                {

                    if (!policyPayload.IsSuccessStatusCode)
                        throw new InvalidOperationException
                            ($"Unable to retrieve policies for project {projectId}.");

                    JToken jt = JToken.Load(new JsonTextReader(new StreamReader
                        (policyPayload.Content.ReadAsStreamAsync().Result)));

                    return GetFlatPolicyNames(jt);
                }
            }
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
            using (var client = ctx.Json.CreateMnoClient())
            using (var policyPayload = client.GetAsync(CxRestContext.MakeUrl(ctx.MnoUrl,
                POLICY_LIST_URL_SUFFIX), token).Result)
            {

                if (!policyPayload.IsSuccessStatusCode)
                    throw new InvalidOperationException
                        ("Unable to retrieve policies.");

                JToken jt = JToken.Load(new JsonTextReader(new StreamReader
                    (policyPayload.Content.ReadAsStreamAsync().Result)));

                return ParsePolicies(ctx, token, jt);
            }
        }


        public static IEnumerable<int> GetPolicyIdsForProject(CxRestContext ctx,
                CancellationToken token, int projectId)
        {
            using (var client = ctx.Json.CreateMnoClient())
            using (var policyPayload = client.GetAsync(CxRestContext.MakeUrl(ctx.MnoUrl,
                String.Format(PROJECT_POLICY_URL_SUFFIX, projectId)), token).Result)
            {

                if (!policyPayload.IsSuccessStatusCode)
                    throw new InvalidOperationException
                        ($"Unable to retrieve policies for project {projectId}.");

                JToken jt = JToken.Load(new JsonTextReader(new StreamReader
                    (policyPayload.Content.ReadAsStreamAsync().Result)));

                LinkedList<int> policyIds = new LinkedList<int>();

                using (JTokenReader reader = new JTokenReader(jt))
                    while (JsonUtils.MoveToNextProperty(reader, "id"))
                    {
                        policyIds.AddLast(Convert.ToInt32(((JProperty)reader.CurrentToken).Value));
                    }

                return policyIds;
            }
        }
    }
}
