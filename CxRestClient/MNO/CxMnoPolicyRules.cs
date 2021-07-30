using log4net;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Threading;
using CxRestClient.Utility;
using CxRestClient.MNO.dto;

namespace CxRestClient.MNO
{
    public class CxMnoPolicyRules
    {
        private static ILog _log = LogManager.GetLogger(typeof(CxMnoPolicyRules));

        private static readonly String URL_SUFFIX = "cxarm/policymanager/policies/{0}/rules";

        private static IEnumerable<RuleDescriptor> ParseRules(CxRestContext ctx,
        CancellationToken token, JToken rulePayload)
        {
            using (var reader = new JTokenReader(rulePayload))
            {
                LinkedList<RuleDescriptor> rules = new LinkedList<RuleDescriptor>();

                while (JsonUtils.MoveToNextProperty(reader, "ruleId"))
                {
                    RuleDescriptor rule = new RuleDescriptor()
                    {
                        RuleId = Convert.ToInt32(((JProperty)reader.CurrentToken).Value)
                    };


                    if (!JsonUtils.MoveToNextProperty(reader, "name"))
                        continue;
                    rule.Name = ((JProperty)reader.CurrentToken).Value.ToString();

                    if (!JsonUtils.MoveToNextProperty(reader, "description"))
                        continue;
                    rule.Description = ((JProperty)reader.CurrentToken).Value.ToString();

                    if (!JsonUtils.MoveToNextProperty(reader, "scanType"))
                        continue;
                    rule.ScanProduct = ((JProperty)reader.CurrentToken).Value.ToString();

                    if (!JsonUtils.MoveToNextProperty(reader, "ruleType"))
                        continue;
                    rule.RuleType = ((JProperty)reader.CurrentToken).Value.ToString();

                    if (!JsonUtils.MoveToNextProperty(reader, "createdOn"))
                        continue;
                    rule.CreatedOn = JsonUtils.UtcEpochTimeToDateTime
                        (Convert.ToInt64(((JProperty)reader.CurrentToken).Value) / 1000);

                    rules.AddLast(rule);
                }

                return rules;
            }
        }


        public static IEnumerable<RuleDescriptor> GetRulesForPolicy(CxRestContext ctx,
        CancellationToken token, int policyId)
        {
			return WebOperation.ExecuteGet<IEnumerable<RuleDescriptor>>(
			ctx.Json.CreateMnoClient
			, (response) =>
			{
				using (var sr = new StreamReader(response.Content.ReadAsStreamAsync().Result))
				using (var jtr = new JsonTextReader(sr))
				{
					JToken jt = JToken.Load(jtr);
					return ParseRules(ctx, token, jt);
				}
			}
			, CxRestContext.MakeUrl(ctx.MnoUrl, String.Format(URL_SUFFIX, policyId))
			, ctx
			, token, apiVersion: null);
        }
    }
}
