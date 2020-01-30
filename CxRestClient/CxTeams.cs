using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace CxRestClient
{
    public class CxTeams
    {
        private static String URL_SUFFIX = "cxrestapi/auth/teams";


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
                TeamId = new Guid (((JProperty)_reader.CurrentToken).Value.ToString()),
                TeamName = ((JProperty)_reader.CurrentToken.Next).Value.ToString()
            };

            object IEnumerator.Current => Current;

            public void Dispose()
            {
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

        public static IEnumerable<Team> GetTeams(CxRestContext ctx)
        {
            var teams = ctx.GetJsonClient().GetAsync(CxRestContext.MakeUrl(ctx.Url, URL_SUFFIX)).Result;

            if (!teams.IsSuccessStatusCode)
                throw new InvalidOperationException(teams.ReasonPhrase);

            JToken jt = JToken.Load(new JsonTextReader(new StreamReader
                (teams.Content.ReadAsStreamAsync().Result)));

            return new TeamReader (jt);
        }


    }
}
