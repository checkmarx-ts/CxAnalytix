using log4net;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Threading;

namespace CxRestClient
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

        public static ScanSettings GetScanSettings(CxRestContext ctx, CancellationToken token, int projectId)
        {
            try
            {
                String restUrl = CxRestContext.MakeUrl(ctx.Url, URL_SUFFIX);

                using (var client = ctx.Json.CreateSastClient())
                using (var settings = client.GetAsync(CxRestContext.MakeUrl(restUrl,
                    Convert.ToString(projectId)), token).Result)
                {
                    if (token.IsCancellationRequested)
                        return null;

                    if (!settings.IsSuccessStatusCode)
                        throw new InvalidOperationException(settings.ReasonPhrase);

                    using (var sr = new StreamReader
                            (settings.Content.ReadAsStreamAsync().Result))
                    using (var jtr = new JsonTextReader(sr))
                    {
                        JToken jt = JToken.Load(jtr);

                        return new ScanSettings(jt);
                    }
                }

            }
            catch (HttpRequestException hex)
            {
                _log.Error("Communication error.", hex);
                throw hex;
            }
        }
    }
}
