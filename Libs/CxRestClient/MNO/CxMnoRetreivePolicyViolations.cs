using log4net;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.IO;
using System.Threading;
using CxRestClient.Utility;
using CxRestClient.MNO.dto;
using CxRestClient.MNO.Collections;

namespace CxRestClient.MNO
{
    public class CxMnoRetreivePolicyViolations
    {
        private static ILog _log = LogManager.GetLogger(typeof(CxMnoRetreivePolicyViolations));

        private static readonly String URL_SUFFIX = "cxarm/policymanager/projects" +
            "/{0}/violations";

        private static ViolatedPolicyCollection ParseViolatedRules
            (PolicyCollection policies, int projectId, JToken token)
        {
            ViolatedPolicyCollection violatedRules = new ViolatedPolicyCollection();

            using (JTokenReader reader = new JTokenReader(token))
            {

                while (reader.Read() && reader.CurrentToken.Type != JTokenType.Array) ;

                if (reader.CurrentToken == null || reader.CurrentToken.Type != JTokenType.Array)
                    return violatedRules;

                JArray policyViolations = (JArray)reader.CurrentToken;

                for (int y = 0; y < policyViolations.Count; y++)
                {
                    if (!JsonUtils.MoveToNextProperty(reader, "policyId"))
                        continue;

                    int currentPolicyId = Convert.ToInt32(((JProperty)reader.CurrentToken).Value);

                    if (!JsonUtils.MoveToNextProperty(reader, "violations"))
                        continue;

                    JArray ruleViolations = (JArray)((JProperty)reader.CurrentToken).Value;

                    for (int x = 0; x < ruleViolations.Count; x++)
                    {

                        if (!JsonUtils.MoveToNextProperty(reader, "ruleId"))
                            break;
                        var ruleId = Convert.ToInt32(((JProperty)reader.CurrentToken).Value);
                        var rule = policies.GetPolicyByRuleId(ruleId).Rules[ruleId];

                        ViolatedRuleDescriptor curRule = new ViolatedRuleDescriptor(rule)
                        {
                            ProjectId = projectId,
                            PolicyId = currentPolicyId
                        };

                        if (!JsonUtils.MoveToNextProperty(reader, "firstDetectionDateByArm"))
                            break;
                        curRule.FirstDetectionDate = JsonUtils.UtcEpochTimeToDateTime
                            (Convert.ToInt64(((JProperty)reader.CurrentToken).Value) / 1000);

                        if (!JsonUtils.MoveToNextProperty(reader, "findingId"))
                            break;
                        curRule.ViolationId = ((JProperty)reader.CurrentToken).Value.ToString();


                        if (!JsonUtils.MoveToNextProperty(reader, "scanId"))
                            break;
                        curRule.ScanId = ((JProperty)reader.CurrentToken).Value.ToString();

                        if (!JsonUtils.MoveToNextProperty(reader, "name"))
                            break;
                        curRule.ViolationName = ((JProperty)reader.CurrentToken).Value.ToString();

                        if (!JsonUtils.MoveToNextProperty(reader, "severity"))
                            break;
                        curRule.ViolationSeverity = ((JProperty)reader.CurrentToken).Value.ToString();

                        if (!JsonUtils.MoveToNextProperty(reader, "date"))
                            break;
                        if (((JProperty)reader.CurrentToken).Value.Type != JTokenType.Null)
                            curRule.ViolationOccured = JsonUtils.UtcEpochTimeToDateTime
                                (Convert.ToInt64(((JProperty)reader.CurrentToken).Value));

                        if (!JsonUtils.MoveToNextProperty(reader, "riskScore"))
                            break;
                        if (((JProperty)reader.CurrentToken).Value.Type != JTokenType.Null)
                            curRule.ViolationRiskScore = Convert.ToDouble
                                (((JProperty)reader.CurrentToken).Value.ToString());


                        if (!JsonUtils.MoveToNextProperty(reader, "status"))
                            break;
                        curRule.ViolationStatus = ((JProperty)reader.CurrentToken).Value.ToString();

                        if (!JsonUtils.MoveToNextProperty(reader, "state"))
                            break;
                        curRule.ViolationState = ((JProperty)reader.CurrentToken).Value.ToString();


                        violatedRules.AddViolatedRule(curRule);
                    }
                }
            }
            return violatedRules;
        }

        public static ViolatedPolicyCollection GetViolations(CxSASTRestContext ctx,
                CancellationToken token, int projectId, PolicyCollection policies)
        {
            return WebOperation.ExecuteGet<ViolatedPolicyCollection>(
                ctx.Mno.Json.CreateClient
                , (response) =>
                {
                    using (var sr = new StreamReader (response.Content.ReadAsStreamAsync().Result))
                    using (var jtr = new JsonTextReader(sr))
                    {
                        JToken jt = JToken.Load(jtr);
                        return ParseViolatedRules(policies, projectId, jt);
                    }
                }
                , UrlUtils.MakeUrl(ctx.Mno.ApiUrl, String.Format(URL_SUFFIX, projectId))
                , ctx.Mno
                , token, apiVersion: null);
        }
    }
}
