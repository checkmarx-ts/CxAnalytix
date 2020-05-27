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
    public class CxSastScans
    {
        private static ILog _log = LogManager.GetLogger(typeof (CxSastScans));

        private static String URL_SUFFIX = "cxrestapi/sast/scans";

        private CxSastScans ()
        { }

        public enum ScanStatus
        {
            All,
            Scanning,
            Finished,
            Canceled,
            Failed
        }

        [JsonObject(MemberSerialization.OptIn)]
        public class Scan
        {
            [JsonProperty(PropertyName = "id")]
            public String ScanId { get; internal set; }
            public int ProjectId { get => Convert.ToInt32(project["id"]); }
            [JsonProperty(PropertyName = "project")]
            internal Dictionary<String, Object> project { get; set; }
            [JsonProperty(PropertyName = "dateAndTime")]
            internal Dictionary<String, String> date_times { get; set; }
            public FormattedDateTime StartTime { get => new FormattedDateTime (date_times["startedOn"]); }
            public FormattedDateTime FinishTime { get => new FormattedDateTime(date_times["finishedOn"]); }
            [JsonProperty(PropertyName = "scanState")]
            internal Dictionary<String, Object> scan_state { get; set; }
            public int FileCount { get => Convert.ToInt32(scan_state["filesCount"]); }
            public long LinesOfCode { get => Convert.ToInt64(scan_state["linesOfCode"]); }
            public long FailedLinesOfCode { get => Convert.ToInt64(scan_state["failedLinesOfCode"]); }
            public String CxVersion { get => Convert.ToString(scan_state["cxVersion"]); }
            public String Languages
            {
                get
                {
                    LinkedList<String> langs = new LinkedList<string>();

                    var langDicts = (List<Dictionary<String, String>>)new JsonSerializer().
                        Deserialize(new JTokenReader(scan_state["languageStateCollection"] as JArray), typeof(List<Dictionary<String, String>>));

                    foreach (var langDict in langDicts)
                        langs.AddLast(langDict["languageName"]);

                    return String.Join(';', langs);
                }
            }

            [JsonProperty(PropertyName = "isPublic")]
            internal bool IsPublic { get; set; }

            [JsonProperty(PropertyName = "isIncremental")]
            internal bool IsIncremental { get; set; }
            public String ScanType { get => IsIncremental ? "Incremental" : "Full"; }

            [JsonProperty(PropertyName = "scanRisk")]
            public int ScanRisk { get; internal set; }
            [JsonProperty(PropertyName = "scanRiskSeverity")]
            public int ScanRiskSeverity { get; internal set; }

        }


        private class ScansReader : IEnumerable<Scan>, IEnumerator<Scan>
        {

            private JToken _json;
            private JTokenReader _reader;
            internal ScansReader(JToken json)
            {
                _json = json;
                _reader = new JTokenReader(_json);
            }

            public Scan Current => _currentScan;

            object IEnumerator.Current => Current;

            public void Dispose()
            {
                _reader = null;
            }

            public IEnumerator<Scan> GetEnumerator()
            {
                return new ScansReader(_json);
            }

            private Scan _currentScan;

            int _arrayPos = 0;
            JArray _scanArray;


            public bool MoveNext()
            {

                while (true)
                {
                    if (_reader.CurrentToken == null)
                    {
                        while (_reader.Read() && _reader.CurrentToken.Type != JTokenType.Array) ;
                        if (_reader.CurrentToken == null || _reader.CurrentToken.Type != JTokenType.Array)
                            return false;

                        _scanArray = (JArray)_reader.CurrentToken;
                    }
                    else
                        _arrayPos++;

                    if (!(_arrayPos < _scanArray.Count))
                        return false;

                    _currentScan = (Scan)new JsonSerializer().
                        Deserialize(new JTokenReader(_scanArray[_arrayPos]), typeof(Scan));

                    if (_currentScan.IsPublic)
                        break;
                }

                return true;
            }

            public void Reset()
            {
                throw new NotImplementedException();
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                return new ScansReader(_json);
            }

        }


        public static IEnumerable<Scan> GetScans(CxRestContext ctx, CancellationToken token)
        {
            return GetScans(ctx, token, ScanStatus.All);
        }

        public static IEnumerable<Scan> GetScans(CxRestContext ctx, CancellationToken token, 
            ScanStatus specificStatus)
        {
            try
            {
                String url = null;

                if (specificStatus != ScanStatus.All)
                {
                    url = CxRestContext.MakeUrl(ctx.Url, URL_SUFFIX, new Dictionary<string, string>()
                {
                    { "scanStatus", specificStatus.ToString () }
                }
                    );
                }
                else
                    url = CxRestContext.MakeUrl(ctx.Url, URL_SUFFIX);
                using (var client = ctx.Json.CreateSastClient())
                {
                    using (var scans = client.GetAsync(url, token).Result)
                    {
                        if (token.IsCancellationRequested)
                            return null;

                        if (!scans.IsSuccessStatusCode)
                            throw new InvalidOperationException(scans.ReasonPhrase);

                        using (var sr = new StreamReader
                            (scans.Content.ReadAsStreamAsync().Result))
                        using (var jtr = new JsonTextReader(sr))
                        {
                            JToken jt = JToken.Load(jtr);
                            return new ScansReader(jt);
                        }
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
