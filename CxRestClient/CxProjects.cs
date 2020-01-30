using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace CxRestClient
{
    public class CxProjects
    {
        private static String URL_SUFFIX = "cxrestapi/projects";

        public struct Project
        {
            public Guid TeamId { get; internal set; }
            public int PresetId { get; internal set; }
            public int ProjectId { get; internal set; }
            public String ProjectName { get; internal set; }
        }

        private class ProjectReader : IEnumerable<Project>, IEnumerator<Project>
        {

            private JToken _json;
            private JTokenReader _reader;
            private CxRestContext _ctx;
            internal ProjectReader(JToken json, CxRestContext ctx)
            {
                _json = json;
                _ctx = ctx;
                _reader = new JTokenReader(_json);
            }

            public Project Current => _curProject;

            object IEnumerator.Current => Current;

            public void Dispose()
            {
            }

            public IEnumerator<Project> GetEnumerator()
            {
                return new ProjectReader(_json, _ctx);
            }

            Project _curProject = new Project();

            private static bool MoveToNextProperty (JTokenReader reader)
            {
                while (reader.Read ())
                {
                    if (reader.CurrentToken.Type == JTokenType.Property)
                        return true;
                }

                return false;
            }

            public bool MoveNext()
            {
                while (_reader.Read())
                    if (_reader.CurrentToken.Type == JTokenType.Property)
                        if (((JProperty)_reader.CurrentToken).Name.CompareTo("id") == 0)
                        {
                            _curProject = new Project()
                            {
                                ProjectId = Convert.ToInt32(((JProperty)_reader.CurrentToken).Value.ToString())
                            };

                            if (!MoveToNextProperty (_reader))
                                return false;

                            _curProject.TeamId = new Guid(((JProperty)_reader.CurrentToken).Value.ToString());

                            if (!MoveToNextProperty(_reader))
                                return false;

                            _curProject.ProjectName = ((JProperty)_reader.CurrentToken).Value.ToString();

                            if (!MoveToNextProperty(_reader))
                                return false;

                            // IsPublic?
                            if (!Convert.ToBoolean(((JProperty)_reader.CurrentToken).Value.ToString()))
                            {
                                // Scan isn't public, move to the next scan.
                                _curProject = new Project();
                                continue;
                            }

                            _curProject.PresetId = CxProjectScanSettings.GetScanSettings
                                (_ctx, _curProject.ProjectId).PresetId;

                            return true;
                        }

                return false;
            }

            public void Reset()
            {
                throw new NotImplementedException();
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                return new ProjectReader(_json, _ctx);
            }

        }


        public static IEnumerable<Project> GetProjects(CxRestContext ctx)
        {
            var projects = ctx.GetJsonClient().GetAsync(CxRestContext.MakeUrl(ctx.Url, URL_SUFFIX)).Result;

            if (!projects.IsSuccessStatusCode)
                throw new InvalidOperationException(projects.ReasonPhrase);

            JToken jt = JToken.Load(new JsonTextReader(new StreamReader
                (projects.Content.ReadAsStreamAsync().Result)));

            return new ProjectReader(jt, ctx);
        }
    }
}
