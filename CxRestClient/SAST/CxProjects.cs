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
    public class CxProjects
    {
        private static ILog _log = LogManager.GetLogger(typeof(CxProjects));

        private static String URL_SUFFIX = "cxrestapi/projects";

        private CxProjects()
        { }

        [JsonObject(MemberSerialization.OptIn)]
        public class ProjectCustomFields
        {
            [JsonProperty(PropertyName = "name")]
            public String FieldName { get; internal set; }
            [JsonProperty(PropertyName = "value")]
            public String FieldValue { get; internal set; }
        }

        [JsonObject(MemberSerialization.OptIn)]
        public class Project
        {
            [JsonProperty(PropertyName = "teamId")]
            public String TeamId { get; internal set; }
            public int PresetId { get; internal set; }
            [JsonProperty(PropertyName = "id")]
            public int ProjectId { get; internal set; }
            [JsonProperty(PropertyName = "name")]
            public String ProjectName { get; internal set; }
            [JsonProperty(PropertyName = "isPublic")]
            public bool IsPublic { get; internal set; }
            [JsonProperty(PropertyName = "customFields")]
            public List<ProjectCustomFields> CustomFields { get; internal set; }

			public override string ToString() =>
                $"{ProjectId}:{ProjectName} [TeamId: {TeamId} Public: {IsPublic} CustomFields: {CustomFields.Count}]";
		}

        private class ProjectReader : IEnumerable<Project>, IEnumerator<Project>, IDisposable
        {

            private JToken _json;
            private JTokenReader _reader;
            private CxRestContext _ctx;
            private CancellationToken _token;
            internal ProjectReader(JToken json, CxRestContext ctx, CancellationToken token)
            {
                _json = json;
                _ctx = ctx;
                _reader = new JTokenReader(_json);
                _token = token;
            }

            public Project Current => _curProject;

            object IEnumerator.Current => Current;

            public void Dispose()
            {
                if (_reader != null)
                {
                    _reader.Close();
                    (_reader as IDisposable).Dispose();
                    _reader = null;
                }
                _ctx = null;
            }

            public IEnumerator<Project> GetEnumerator()
            {
                return new ProjectReader(_json, _ctx, _token);
            }

            private Project _curProject;

            int _arrayPos = 0;
            JArray _projectArray;


            public bool MoveNext()
            {

                while (true)
                {
                    if (_reader.CurrentToken == null)
                    {
                        while (_reader.Read() && _reader.CurrentToken.Type != JTokenType.Array);
                        if (_reader.CurrentToken == null || _reader.CurrentToken.Type != JTokenType.Array)
                            return false;

                        _projectArray = (JArray)_reader.CurrentToken;
                    }
                    else
                        _arrayPos++;

                    if (!(_arrayPos < _projectArray.Count))
                        return false;

					using (var jtr = new JTokenReader(_projectArray[_arrayPos]))
						_curProject = (Project)new JsonSerializer().Deserialize(jtr, typeof(Project));

					if (!_curProject.IsPublic)
                    {
                        _curProject = null;
                        continue;
                    }
                    else
                    {
                        _curProject.PresetId = CxProjectScanSettings.GetScanSettings
                        (_ctx, _token, _curProject.ProjectId).PresetId;
                        break;
                    }
                }

                return true;
            }


            public void Reset()
            {
                throw new NotImplementedException();
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                return new ProjectReader(_json, _ctx, _token);
            }

        }


        public static IEnumerable<Project> GetProjects(CxRestContext ctx, CancellationToken token)
        {
            using (var projectReader = WebOperation.ExecuteGet<ProjectReader>(
            ctx.Json.CreateSastClient
            , (response) =>
            {
                using (var sr = new StreamReader(response.Content.ReadAsStreamAsync().Result))
                using (var jtr = new JsonTextReader(sr))
                {
                    JToken jt = JToken.Load(jtr);

                    return new ProjectReader(jt, ctx, token);
                }
            }
            , CxRestContext.MakeUrl(ctx.Url, URL_SUFFIX)
            , ctx
            , token, apiVersion: "2.0"))
                return new List<Project>(projectReader);
        }
    }
}
