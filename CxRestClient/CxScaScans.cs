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

namespace CxRestClient
{
    public class CxScaScans
    {
        private static ILog _log = LogManager.GetLogger(typeof (CxScaScans));

        private static String URL_SUFFIX = "cxrestapi/osa/scans";

        private CxScaScans()
        { }

        public struct Scan
        {
            public int ProjectId { get; internal set; }
            public String ScanId { get; internal set; }
            public DateTime FinishTime { get; internal set; }
            public DateTime StartTime { get; internal set; }

            public override string ToString()
            {
                return JsonConvert.SerializeObject(this, Formatting.None);
            }
        }


        private class ScansReader : IEnumerable<Scan>, IEnumerator<Scan>
        {

            private JToken _json;
            private JTokenReader _reader;
            private int _projectId;
            internal ScansReader(JToken json, int projectId)
            {
                _json = json;
                _reader = new JTokenReader(_json);
                _projectId = projectId;
            }
            internal ScansReader()
            {
                _json = null;
            }

            public Scan Current => _currentScan;

            object IEnumerator.Current => Current;

            public void Dispose()
            {
                _reader = null;
            }

            public IEnumerator<Scan> GetEnumerator()
            {
                if (_json == null)
                    return new ScansReader();

                return new ScansReader(_json, _projectId);
            }

            Scan _currentScan = new Scan();

            public bool MoveNext()
            {
                while (_reader != null && JsonUtils.MoveToNextProperty(_reader))
                {
                    if (((JProperty)_reader.CurrentToken).Name.CompareTo("id") == 0)
                    {
                        _currentScan = new Scan()
                        {
                            ProjectId = _projectId,
                            ScanId = ((JProperty)_reader.CurrentToken).Value.ToString()
                        };

                        if (!JsonUtils.MoveToNextProperty(_reader, "startAnalyzeTime"))
                            return false;

                        // SCA stores times in UTC.  SAST stores them in local time.
                        _currentScan.StartTime = DateTime.Parse(((JProperty)_reader.CurrentToken).
                            Value.ToString()).ToLocalTime();

                        if (!JsonUtils.MoveToNextProperty(_reader, "endAnalyzeTime"))
                            return false;

                        _currentScan.FinishTime = DateTime.Parse(((JProperty)_reader.CurrentToken).
                            Value.ToString()).ToLocalTime();

                        if (!JsonUtils.MoveToNextProperty(_reader, "state"))
                            return false;

                        if (!JsonUtils.MoveToNextProperty(_reader, "name"))
                            return false;

                        return true;
                    }
                }
                return false;
            }

            public void Reset()
            {
                throw new NotImplementedException();
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                return new ScansReader(_json, _projectId);
            }

        }


        public static IEnumerable<Scan> GetScans(CxRestContext ctx, CancellationToken token,
            int projectId)
        {
            try
            {
                String url = CxRestContext.MakeUrl(ctx.Url, URL_SUFFIX, new Dictionary<String, String>()
                    {
                        {"projectId", Convert.ToString (projectId)  },
                        {"itemsPerPage", "5000" }
                    });

                using (var client = ctx.Json.CreateSastClient())
                using (var scans = client.GetAsync(url, token).Result)
                {

                    if (token.IsCancellationRequested)
                        return null;

                    // If no OSA license, result is 403.  Return an empty set of scans.
                    if (scans.StatusCode == System.Net.HttpStatusCode.Forbidden)
                        return new ScansReader();

                    if (!scans.IsSuccessStatusCode)
                        throw new InvalidOperationException(scans.ReasonPhrase);

                    using (var sr = new StreamReader
                            (scans.Content.ReadAsStreamAsync().Result))
                    using (var jtr = new JsonTextReader(sr))
                    {
                        JToken jt = JToken.Load(jtr);
                        return new ScansReader(jt, projectId);
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
