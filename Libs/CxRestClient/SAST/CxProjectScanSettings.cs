using CxRestClient.Utility;
using log4net;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Threading;

namespace CxRestClient.SAST
{
    public class CxProjectScanSettings
    {
        private static ILog _log = LogManager.GetLogger(typeof (CxProjectScanSettings));
        private static String URL_SUFFIX = "cxrestapi/sast/scanSettings";

        private CxProjectScanSettings()
        { }

        public class ScanSettings
        {
            private JToken _json;
            internal ScanSettings(JToken json)
            {
                _json = json;
            }

            private int? _presetId = null;
            public int PresetId
            {
                get
                {
                    if (_presetId == null)
                    {
                        using (var reader = new JTokenReader(_json))
                        {
                            bool foundPreset = false;

                            while (reader.Read())
                            {
                                if (reader.CurrentToken.Type == JTokenType.Property)
                                {
                                    if (!foundPreset)
                                    {
                                        if (((JProperty)reader.CurrentToken).Name.CompareTo("preset") == 0)
                                            foundPreset = true;
                                        continue;
                                    }
                                    else
                                    {
                                        _presetId = Convert.ToInt32(((JProperty)reader.CurrentToken).Value);
                                        break;
                                    }
                                }
                            }
                        }
                    }

                    return _presetId.Value;
                }
            }
        }

        public static ScanSettings GetScanSettings(CxSASTRestContext ctx, CancellationToken token, int projectId)
        {
            String restUrl = UrlUtils.MakeUrl(ctx.Sast.ApiUrl, URL_SUFFIX);

            return WebOperation.ExecuteGet<ScanSettings>(
                ctx.Sast.Json.CreateClient
                , (response) =>
                {
                    using (var sr = new StreamReader(response.Content.ReadAsStreamAsync().Result))
                    using (var jtr = new JsonTextReader(sr))
                    {
                        JToken jt = JToken.Load(jtr);
                        return new ScanSettings(jt);
                    }
                }
                , UrlUtils.MakeUrl(restUrl, Convert.ToString(projectId))
                , ctx.Sast
                , token);
        }
    }
}
