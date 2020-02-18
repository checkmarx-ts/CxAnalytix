using log4net;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.IO;
using System.Text;
using System.Threading;

namespace CxRestClient
{
    public class CxMnoPolicies
    {
        private static ILog _log = LogManager.GetLogger(typeof(CxMnoPolicies));
        private static String URL_SUFFIX = "cxarm/policymanager/projects/{0}/policies";

        private CxMnoPolicies()
        { }


        private static String GetPolicies(JToken payload)
        {
            var reader = new JTokenReader(payload);

            StringBuilder b = new StringBuilder();

            while (JsonUtils.MoveToNextProperty(reader, "name"))
            {
                var policyName = ((JProperty)reader.CurrentToken).Value.ToString();


                if (JsonUtils.MoveToNextProperty(reader, "isActive"))
                {
                    if (Boolean.Parse (((JProperty)reader.CurrentToken).Value.ToString()))
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

        public static String GetProjectPoliciesSingleField(CxRestContext ctx,
                CancellationToken token, int projectId)
        {
            var client = ctx.Json.CreateMnoClient();

            var policyPayload = client.GetAsync(CxRestContext.MakeUrl(ctx.MnoUrl,
                String.Format(URL_SUFFIX, projectId))).Result;

            if (!policyPayload.IsSuccessStatusCode)
                throw new InvalidOperationException
                    ($"Unable to retrieve policies for project {projectId}.");

            JToken jt = JToken.Load(new JsonTextReader(new StreamReader
                (policyPayload.Content.ReadAsStreamAsync().Result)));

            return GetPolicies (jt);
        }

    }
}
