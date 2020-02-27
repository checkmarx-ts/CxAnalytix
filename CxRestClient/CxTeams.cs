using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;

namespace CxRestClient
{
    public class CxTeams
    {
        private static String URL_SUFFIX = "cxrestapi/auth/teams";

        private CxTeams()
        { }

        private class TeamReader : IEnumerable<Team>, IEnumerator<Team>
        {

            private JToken _json;
            private JTokenReader _reader;
            internal TeamReader(JToken json)
            {
                _json = json;
                _reader = new JTokenReader(_json);
            }

            public Team Current => new Team()
            {
                TeamId = new Guid(((JProperty)_reader.CurrentToken).Value.ToString()),
                TeamName = ((JProperty)_reader.CurrentToken.Next).Value.ToString()
            };

            object IEnumerator.Current => Current;

            public void Dispose()
            {
                _reader = null;
            }

            public IEnumerator<Team> GetEnumerator()
            {
                return new TeamReader(_json);
            }

            public bool MoveNext()
            {
                bool read_result = false;

                while (read_result = _reader.Read())
                    if (_reader.CurrentToken.Type == JTokenType.Property)
                        if (((JProperty)_reader.CurrentToken).Name.CompareTo("id") == 0)
                            return true;

                return read_result;
            }

            public void Reset()
            {
                throw new NotImplementedException();
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                return new TeamReader(_json);
            }
        }

        public struct Team
        {
            public Guid TeamId { get; internal set; }
            public String TeamName { get; internal set; }
        }

        public static IEnumerable<Team> GetTeams(CxRestContext ctx, CancellationToken token)
        {
            using (var client = ctx.Json.CreateSastClient())
            using (var teams = client.GetAsync(
                CxRestContext.MakeUrl(ctx.Url, URL_SUFFIX), token).Result)
            {
                if (token.IsCancellationRequested)
                    return null;

                if (!teams.IsSuccessStatusCode)
                    throw new InvalidOperationException(teams.ReasonPhrase);

                using (var sr = new StreamReader
                    (teams.Content.ReadAsStreamAsync().Result))
                using (var jtr = new JsonTextReader(sr))
                {
                    JToken jt = JToken.Load(jtr);
                    return new TeamReader(jt);
                }
            }
        }
    }
}
