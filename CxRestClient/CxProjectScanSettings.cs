using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace CxRestClient
{
    public class CxProjectScanSettings
    {

        private static String URL_SUFFIX = "cxrestapi/sast/scanSettings";

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
                        var reader = new JTokenReader(_json);

                        bool foundPreset = false;

                        while (reader.Read () )
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

                    return _presetId.Value;

                }
            }
        }

        public static ScanSettings GetScanSettings(CxRestContext ctx, int projectId)
        {
            String restUrl = CxRestContext.MakeUrl(ctx.Url, URL_SUFFIX);

            var settings = ctx.GetJsonClient().GetAsync(CxRestContext.MakeUrl(restUrl,
                Convert.ToString(projectId))).Result;

            if (!settings.IsSuccessStatusCode)
                throw new InvalidOperationException(settings.ReasonPhrase);

            JToken jt = JToken.Load(new JsonTextReader(new StreamReader
                (settings.Content.ReadAsStreamAsync().Result)));

            return new ScanSettings(jt);
        }

    }
}
