using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace CxRestClient
{
    public class CxSastScans
    {
        private static String URL_SUFFIX = "cxrestapi/sast/scans";

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
            }

            public IEnumerator<Scan> GetEnumerator()
            {
                return new ScansReader(_json);
            }

            private static bool MoveToNextProperty(JTokenReader reader)
            {
                while (reader.Read())
                {
                    if (reader.CurrentToken.Type == JTokenType.Property)
                        return true;
                }

                return false;
            }

            private static bool MoveToNextProperty(JTokenReader reader, String named)
            {
                while (MoveToNextProperty(reader))
                {
                    if (((JProperty)reader.CurrentToken).Name.CompareTo(named) == 0)
                        return true;
                }

                return false;
            }

            Scan _currentScan = new Scan();

            public bool MoveNext()
            {
                while (MoveToNextProperty(_reader))
                {
                    if (((JProperty)_reader.CurrentToken).Name.CompareTo("id") == 0)
                    {
                        _currentScan = new Scan()
                        {
                            ScanId = ((JProperty)_reader.CurrentToken).Value.ToString()
                        };


                        if (!MoveToNextProperty(_reader, "project"))
                            return false;

                        if (!MoveToNextProperty(_reader, "id"))
                            return false;

                        _currentScan.ProjectId = Convert.ToInt32(((JProperty)_reader.CurrentToken).Value.ToString());

                        if (!MoveToNextProperty(_reader, "scanType"))
                            return false;

                        if (!MoveToNextProperty(_reader, "value"))
                            return false;

                        _currentScan.ScanType = ((JProperty)_reader.CurrentToken).Value.ToString();

                        if (!MoveToNextProperty(_reader, "dateAndTime"))
                            return false;

                        if (!MoveToNextProperty(_reader, "finishedOn"))
                            return false;

                        _currentScan.FinishTime = DateTime.Parse(((JProperty)_reader.CurrentToken).Value.ToString());

                        if (!MoveToNextProperty(_reader, "isPublic"))
                            return false;

                        bool isPublic = Convert.ToBoolean(((JProperty) _reader.CurrentToken).Value.ToString());

                        if (!MoveToNextProperty(_reader, "partialScanReasons"))
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


        public static IEnumerable<Scan> GetScans(CxRestContext ctx)
        {
            return GetScans(ctx, ScanStatus.All);
        }

        public static IEnumerable<Scan> GetScans(CxRestContext ctx, ScanStatus specificStatus)
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

            var scans = ctx.GetJsonClient().GetAsync(url).Result;


            if (!scans.IsSuccessStatusCode)
                throw new InvalidOperationException(scans.ReasonPhrase);

            JToken jt = JToken.Load(new JsonTextReader(new StreamReader
                (scans.Content.ReadAsStreamAsync().Result)));

            return new ScansReader(jt);
        }
    }
}
