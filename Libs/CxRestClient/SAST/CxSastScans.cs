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

namespace CxRestClient.SAST
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
            public DateTime StartTime { get => JsonUtils.NormalizeDateParse (date_times["startedOn"]); }
            public DateTime FinishTime { get => JsonUtils.NormalizeDateParse (date_times["finishedOn"]); }
            public DateTime EngineStartTime { get => JsonUtils.NormalizeDateParse (date_times["engineStartedOn"]); }
            public DateTime EngineFinishTime { get => JsonUtils.NormalizeDateParse (date_times["engineFinishedOn"]); }
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

                    using (var jtr = new JTokenReader(scan_state["languageStateCollection"] as JArray))
                    {
                        var langDicts = (List<Dictionary<String, String>>)new JsonSerializer().Deserialize(jtr, typeof(List<Dictionary<String, String>>));

                        foreach (var langDict in langDicts)
                            langs.AddLast(langDict["languageName"]);

                        return String.Join(';', langs);
                    }
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

            [JsonProperty(PropertyName = "engineServer")]
            internal Dictionary<String, Object> engine { get; set; }

            public String Engine { get => (engine != null) ? (engine["name"] as String) : ("NotSpecified"); }


            public override string ToString()
            {
                return JsonConvert.SerializeObject(this, Formatting.None);
            }
        }


        private class ScansReader : IEnumerable<Scan>, IEnumerator<Scan>, IDisposable
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
                if (_reader != null)
                {
                    _reader.Close();
                    (_reader as IDisposable).Dispose();
                    _reader = null;
                }
            }

            IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
            public IEnumerator<Scan> GetEnumerator() => this;

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

                    _currentScan = (Scan)new JsonSerializer()
                        .Deserialize(new JTokenReader(_scanArray[_arrayPos]), typeof(Scan));

                    if (_currentScan.IsPublic)
                        break;
                }

                return true;
            }

            public void Reset()
            {
                throw new NotImplementedException();
            }


        }


        public static IEnumerable<Scan> GetScans(CxSASTRestContext ctx, CancellationToken token)
        {
            return GetScans(ctx, token, ScanStatus.All);
        }

		public static IEnumerable<Scan> GetScans(CxSASTRestContext ctx, CancellationToken token,
			ScanStatus specificStatus, int? projectId = null)
		{
			String url = null;

			var args = new Dictionary<string, string>();

			if (specificStatus != ScanStatus.All)
				args.Add("scanStatus", specificStatus.ToString());

			if (projectId != null && projectId.HasValue)
				args.Add("projectId", Convert.ToString(projectId.Value));

			url = UrlUtils.MakeUrl(ctx.Sast.ApiUrl, URL_SUFFIX, args);

			using (var scansReader = WebOperation.ExecuteGet<ScansReader>(
			ctx.Sast.Json.CreateClient
			, (response) =>
			{
				using (var sr = new StreamReader(response.Content.ReadAsStreamAsync().Result))
				using (var jtr = new JsonTextReader(sr))
				{
					jtr.DateParseHandling = DateParseHandling.None;
					JToken jt = JToken.Load(jtr);
					return new ScansReader(jt);
				}
			}
			, url
			, ctx.Sast
			, token))
				return new List<Scan>(scansReader);
		}
	}
}
