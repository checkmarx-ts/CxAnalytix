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

        public struct Scan
        {
            public int ProjectId { get; internal set; }
            public String ScanType { get; internal set; }
            public String ScanId { get; internal set; }
            public DateTime FinishTime { get; internal set; }
            public DateTime StartTime { get; internal set; }
            public long LinesOfCode { get; internal set; }
            public long FailedLinesOfCode { get; internal set; }
            public int FileCount { get; internal set; }
            public String CxVersion { get; internal set; }
            public int ScanRisk { get; internal set; }
            public int ScanRiskSeverity { get; internal set; }
            public String Languages { get; internal set; }


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

            private String GetLanguages(JToken languageArray)
            {
                LinkedList<String> langs = new LinkedList<string>();

                JArray array = ((JProperty)languageArray).Value as JArray;

                foreach (JProperty token in array.Values())
                    if (token.Name.CompareTo("languageName") == 0)
                        langs.AddLast(token.Value.ToString());

                return String.Join(';', langs);
            }

            Scan _currentScan = new Scan();

            public bool MoveNext()
            {
                while (JsonUtils.MoveToNextProperty(_reader))
                {
                    if (((JProperty)_reader.CurrentToken).Name.CompareTo("id") == 0)
                    {
                        _currentScan = new Scan()
                        {
                            ScanId = ((JProperty)_reader.CurrentToken).Value.ToString()
                        };


                        if (!JsonUtils.MoveToNextProperty(_reader, "project"))
                            return false;

                        if (!JsonUtils.MoveToNextProperty(_reader, "id"))
                            return false;

                        _currentScan.ProjectId = Convert.ToInt32(((JProperty)_reader.CurrentToken).Value.ToString());

                        if (!JsonUtils.MoveToNextProperty(_reader, "dateAndTime"))
                            return false;

                        if (!JsonUtils.MoveToNextProperty(_reader, "startedOn"))
                            return false;

                        _currentScan.StartTime = DateTime.Parse(((JProperty)_reader.CurrentToken).Value.ToString());

                        if (!JsonUtils.MoveToNextProperty(_reader, "finishedOn"))
                            return false;

                        _currentScan.FinishTime = DateTime.Parse(((JProperty)_reader.CurrentToken).Value.ToString());

                        if (!JsonUtils.MoveToNextProperty(_reader, "scanState"))
                            return false;

                        if (!JsonUtils.MoveToNextProperty(_reader, "filesCount"))
                            return false;
                        _currentScan.FileCount = Convert.ToInt32 (((JProperty)_reader.CurrentToken).Value);

                        if (!JsonUtils.MoveToNextProperty(_reader, "linesOfCode"))
                            return false;

                        _currentScan.LinesOfCode = Convert.ToInt64(((JProperty)_reader.CurrentToken).Value);

                        if (!JsonUtils.MoveToNextProperty(_reader, "failedLinesOfCode"))
                            return false;

                        _currentScan.FailedLinesOfCode = Convert.ToInt64(((JProperty)_reader.CurrentToken).Value);

                        if (!JsonUtils.MoveToNextProperty(_reader, "cxVersion"))
                            return false;

                        _currentScan.CxVersion = ((JProperty)_reader.CurrentToken).Value.ToString ();

                        if (!JsonUtils.MoveToNextProperty(_reader, "languageStateCollection"))
                            return false;

                        _currentScan.Languages = GetLanguages(_reader.CurrentToken);

                        if (!JsonUtils.MoveToNextProperty(_reader, "isPublic"))
                            return false;

                        bool isPublic = Convert.ToBoolean(((JProperty) _reader.CurrentToken).Value.ToString());

                        if (!JsonUtils.MoveToNextProperty(_reader, "isIncremental"))
                            return false;

                        if (Convert.ToBoolean(((JProperty)_reader.CurrentToken).Value.ToString()))
                            _currentScan.ScanType = "Incremental";
                        else
                            _currentScan.ScanType = "Full";


                        if (!JsonUtils.MoveToNextProperty(_reader, "scanRisk"))
                            return false;

                        _currentScan.ScanRisk = Convert.ToInt32(((JProperty)_reader.CurrentToken).Value);

                        if (!JsonUtils.MoveToNextProperty(_reader, "scanRiskSeverity"))
                            return false;

                        _currentScan.ScanRiskSeverity = Convert.ToInt32(((JProperty)_reader.CurrentToken).Value);

                        if (!JsonUtils.MoveToNextProperty(_reader, "partialScanReasons"))
                            return false;

                        // IsPublic?
                        if (!isPublic)
                        {
                            // Scan isn't public, move to the next scan.
                            _currentScan = new Scan();
                            continue;
                        }

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
