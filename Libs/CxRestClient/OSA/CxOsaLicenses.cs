using CxRestClient.Utility;
using log4net;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Threading;

namespace CxRestClient.OSA
{
    public class CxOsaLicenses
    {
        private static ILog _log = LogManager.GetLogger(typeof (CxOsaLicenses));

        private static String URL_SUFFIX = "cxrestapi/osa/licenses";

        private CxOsaLicenses()
        { }

        public class License
        {
            [JsonProperty(PropertyName = "id")]
            public String LicenseId { get; internal set; }

            [JsonProperty(PropertyName = "name")]
            public String LicenseName { get; internal set; }

            [JsonProperty(PropertyName = "riskLevel")]
            public String RiskLevel { get; internal set; }

            [JsonProperty(PropertyName = "copyrightRiskScore")]
            public String CopyrightRiskScore { get; internal set; }

            [JsonProperty(PropertyName = "patentRiskScore")]
            public String PatentRiskScore { get; internal set; }

            [JsonProperty(PropertyName = "copyLeft")]
            public String CopyLeft { get; internal set; }

            [JsonProperty(PropertyName = "linking")]
            public String Linking { get; internal set; }

            [JsonProperty(PropertyName = "royalityFree")]
            public String RoyaltyFree { get; internal set; }
            [JsonProperty(PropertyName = "referenceType")]
            public String ReferenceType { get; internal set; }
            [JsonProperty(PropertyName = "reference")]
            public String Reference { get; internal set; }
            [JsonProperty(PropertyName = "url")]
            public String Url { get; internal set; }
        }

        private class LicensesReader : IEnumerable<License>, IEnumerator<License>, IDisposable
        {
            private JToken _json;
            private JTokenReader _reader;

            internal LicensesReader(JToken json)
            {
                _json = json;
                _reader = new JTokenReader(_json);
            }

            private License _currentLicense;

            public License Current => _currentLicense;

            object IEnumerator.Current => Current;

            public void Dispose()
            {
                if (_reader != null)
                {
                    _reader.Close();
                    (_reader as IDisposable).Dispose();
                    _reader = null;
                }
            }

            public IEnumerator<License> GetEnumerator()
            {
                return new LicensesReader(_json);

            }

            int _arrayPos = 0;
            JArray _licenseArray;

            public bool MoveNext()
            {

                if (_reader.CurrentToken == null)
                {
                    while (_reader.Read() && _reader.CurrentToken.Type != JTokenType.Array) ;
                    if (_reader.CurrentToken == null || _reader.CurrentToken.Type != JTokenType.Array)
                        return false;

                    _licenseArray = (JArray)_reader.CurrentToken;
                }
                else
                    _arrayPos++;

                if (!(_arrayPos < _licenseArray.Count))
                    return false;

                using (var jtr = new JTokenReader(_licenseArray[_arrayPos]))
                _currentLicense = (License)new JsonSerializer().Deserialize(jtr, typeof(License));

                return true;
            }

            public void Reset()
            {
                throw new NotImplementedException();
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                return GetEnumerator();
            }
        }

        public static IEnumerable<License> GetLicenses(CxSASTRestContext ctx, CancellationToken token,
        String scanId)
        {

            String url = UrlUtils.MakeUrl(ctx.Sast.ApiUrl, URL_SUFFIX, new Dictionary<String, String>()
                {
                    {"scanId", Convert.ToString (scanId)  }
                });

            List<License> retVal = new List<License>();

            using (var licReader = WebOperation.ExecuteGet<LicensesReader>(
            ctx.Sast.Json.CreateClient
            , (response) =>
            {
                using (var sr = new StreamReader(response.Content.ReadAsStreamAsync().Result))
                using (var jtr = new JsonTextReader(sr))
                {
                    JToken jt = JToken.Load(jtr);
                    return new LicensesReader(jt);
                }
            }
            , url
            , ctx.Sast
            , token))
            {
                retVal.AddRange(licReader);
                return retVal;
            }

        }
    }
}
