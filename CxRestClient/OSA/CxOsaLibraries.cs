using log4net;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Runtime.CompilerServices;
using System.Threading;

namespace CxRestClient.OSA
{
    public class CxOsaLibraries
    {
        private static ILog _log = LogManager.GetLogger(typeof (CxOsaLibraries) );
        private static readonly String URL_SUFFIX = "cxrestapi/osa/libraries";
        private static readonly String PAGE_SIZE = "250";


        private CxOsaLibraries ()
        { }

        public class Library
        {
            public class Match
            {
                [JsonProperty(PropertyName = "name")]
                public String MatchType { get; internal set; }
                [JsonProperty(PropertyName = "description")]
                public String MatchTypeDescription { get; internal set; }

            }

            [JsonProperty(PropertyName = "id")]
            public String LibraryId { get; internal set; }
            [JsonProperty(PropertyName = "name")]
            public String LibraryName { get; internal set; }
            [JsonProperty(PropertyName = "version")]
            public String LibraryVersion { get; internal set; }
            [JsonProperty(PropertyName = "releaseDate")]
            public String ReleaseDate { get; internal set; }
            [JsonProperty(PropertyName = "highUniqueVulnerabilityCount")]
            public int HighVulnerabilities { get; internal set; }
            [JsonProperty(PropertyName = "mediumUniqueVulnerabilityCount")]
            public int MediumVulnerabilities { get; internal set; }
            [JsonProperty(PropertyName = "lowUniqueVulnerabilityCount")]
            public int LowVulnerabilities { get; internal set; }
            [JsonProperty(PropertyName = "newestVersion")]
            public String LatestVersion { get; internal set; }
            [JsonProperty(PropertyName = "newestVersionReleaseDate")]
            public String LatestVersionReleased { get; internal set; }
            [JsonProperty(PropertyName = "numberOfVersionsSinceLastUpdate")]
            public int NumVersionsSinceLastUpdate { get; internal set; }
            [JsonProperty(PropertyName = "confidenceLevel")]
            public int ConfidenceLevel { get; internal set; }
            [JsonProperty(PropertyName = "matchType")]
            public Match MatchType { get; internal set; }
            [JsonProperty(PropertyName = "licenses")]
            public LinkedList<String> Licenses { get; internal set; }
            [JsonProperty(PropertyName = "outdated")]
            public bool Outdated { get; internal set; }
        }


        private class LibrariesReader : IEnumerable<Library>, IEnumerator<Library>
        {
            private JToken _json;
            private JTokenReader _reader;

            internal LibrariesReader(JToken json)
            {
                _json = json;
                _reader = new JTokenReader(_json);
            }

            private Library _currentLibrary;

            public Library Current => _currentLibrary;

            object IEnumerator.Current => Current;

            public void Dispose()
            {
                _reader = null;
            }

            public IEnumerator<Library> GetEnumerator()
            {
                return new LibrariesReader(_json);
            }

            int _arrayPos = 0;
            JArray _libArray;

            public bool MoveNext()
            {

                if (_reader.CurrentToken == null)
                {
                    while (_reader.Read() && _reader.CurrentToken.Type != JTokenType.Array) ;
                    if (_reader.CurrentToken == null || _reader.CurrentToken.Type != JTokenType.Array)
                        return false;

                    _libArray = (JArray)_reader.CurrentToken;
                }
                else
                    _arrayPos++;

                if (!(_arrayPos < _libArray.Count))
                    return false;

                _currentLibrary = (Library)new JsonSerializer().
                    Deserialize(new JTokenReader(_libArray[_arrayPos]), typeof(Library));

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


        public static IEnumerable<Library> GetLibraries (CxRestContext ctx, CancellationToken token,
        String scanId)
        {
            int curPage = 1;

            List<Library> returnLibs = new List<Library>();

            try
            {
                Func<int, String> url = (pg) => CxRestContext.MakeUrl(ctx.Url, URL_SUFFIX, new Dictionary<String, String>()
                {
                    {"scanId", Convert.ToString (scanId)  },
                    { "page", Convert.ToString (pg) },
                    { "itemsPerPage", PAGE_SIZE}
                });

                while (true)
                {
                    using (var client = ctx.Json.CreateSastClient())
                    using (var libraryResponse = client.GetAsync(url(curPage++), token).Result)
                    {

                        if (token.IsCancellationRequested)
                            return null;

                        if (!libraryResponse.IsSuccessStatusCode)
                            throw new InvalidOperationException(libraryResponse.ReasonPhrase);

                        using (var sr = new StreamReader
                            (libraryResponse.Content.ReadAsStreamAsync().Result))
                        using (var jtr = new JsonTextReader(sr))
                        {
                            JToken jt = JToken.Load(jtr);
                            var beforeCount = returnLibs.Count;
                            returnLibs.AddRange(new LibrariesReader(jt));
                            if (returnLibs.Count == beforeCount)
                                break;
                        }
                    }
                }

                return returnLibs;
            }
            catch (HttpRequestException hex)
            {
                _log.Error("Communication error.", hex);
                throw hex;
            }
        }
    }
}
