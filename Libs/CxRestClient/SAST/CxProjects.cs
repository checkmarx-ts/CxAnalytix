using CxRestClient.Utility;
using log4net;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Reflection.Metadata;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;

namespace CxRestClient.SAST
{
    public class CxProjects
    {
        private static ILog _log = LogManager.GetLogger(typeof(CxProjects));

        private static readonly String REST_URL_SUFFIX = "cxrestapi/projects";

        private static readonly int ODATA_TOP = 25;
        private static readonly String ODATA_URL_SUFFIX = "cxwebinterface/odata/v1/Projects?$expand=CustomFields&$select=OwningTeamId,PresetId,Id,Name,IsPublic" +
            $"&$orderby=Id asc&$top={ODATA_TOP}";

        private static String _apiVersion = null;

        [MethodImpl(MethodImplOptions.Synchronized)]
        private static String GetApiVersion(CxSASTRestContext ctx, CancellationToken token)
        {
            if (_apiVersion == null)
            {
                var mm = CxVersion.GetServerMajorMinorVersion(ctx, token);

                if (mm.IsUnknown)
                    _apiVersion = "2.0";
                else if (mm.Major >= 9)
                    _apiVersion = "2.2";
            }

            return _apiVersion;
        }

        private CxProjects()
        { }

        #region DTOs

        [JsonObject(MemberSerialization.OptIn)]
        public class ProjectCustomFields
        {
            [JsonProperty(PropertyName = "name")]
            public String FieldName { get; internal set; }

            [JsonProperty(PropertyName = "FieldName")]
            private String odata_FieldName
            {
                get => FieldName;
                set => FieldName = value;
            }


            [JsonProperty(PropertyName = "value")]
            public String FieldValue { get; internal set; }

            [JsonProperty(PropertyName = "FieldValue")]
            private String odata_FieldValue
            {
                get => FieldValue;
                set => FieldValue = value;
            }

        }

        [JsonObject(MemberSerialization.OptIn)]
        public class Project
        {
            [JsonProperty(PropertyName = "teamId")]
            public String TeamId { get; internal set; }
            [JsonProperty(PropertyName = "OwningTeamId")]
            private String odata_TeamId
            {
                get => TeamId;
                set => TeamId = value;
            }



            public int PresetId { get; internal set; }


            [JsonProperty(PropertyName = "id")]
            public int ProjectId { get; internal set; }
            [JsonProperty(PropertyName = "Id")]
            private int odata_ProjectId
            {
                get => ProjectId;
                set => ProjectId = value;
            }


            [JsonProperty(PropertyName = "name")]
            public String ProjectName { get; internal set; }
            [JsonProperty(PropertyName = "Name")]
            private String odata_ProjectName
            {
                get => ProjectName;
                set => ProjectName = value;
            }




            [JsonProperty(PropertyName = "isPublic")]
            public bool IsPublic { get; internal set; }
            [JsonProperty(PropertyName = "IsPublic")]
            private bool odata_IsPublic
            {
                get => IsPublic;
                set => IsPublic = value;
            }



            [JsonProperty(PropertyName = "customFields")]
            public List<ProjectCustomFields> CustomFields { get; internal set; }
            [JsonProperty(PropertyName = "CustomFields")]
            private List<ProjectCustomFields> odata_CustomFields
            {
                get => CustomFields;
                set => CustomFields = value;
            }



            [JsonProperty(PropertyName = "isBranched")]
            public bool IsBranched { get; internal set; }

            [JsonProperty(PropertyName = "originalProjectId")]
            public String BranchParentProject { get; internal set; }

            [JsonProperty(PropertyName = "branchedOnScanId")]
            public String BranchedAtScanId { get; internal set; }


            public override string ToString() =>
                $"{ProjectId}:{ProjectName} [TeamId: {TeamId} Public: {IsPublic} CustomFields: {CustomFields.Count}]";
		}
        #endregion

        private class ProjectReader : IEnumerable<Project>, IEnumerator<Project>, IDisposable
        {

            private JToken _json;
            private JTokenReader _reader;
            private CxSASTRestContext _ctx;
            private CancellationToken _token;
            internal ProjectReader(JToken json, CxSASTRestContext ctx, CancellationToken token)
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

            IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
            public IEnumerator<Project> GetEnumerator() => this;

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
        }

        private static IEnumerable<Project> GetProjects_odata(CxSASTRestContext ctx, CancellationToken token)
        {
            List<Project> returnedResults = new();

            var filter = new Dictionary<String, String>();
            List<Project> fetchedPage = null;

            do
            {
                String requestUrl = UrlUtils.MakeUrl(ctx.Sast.ApiUrl, ODATA_URL_SUFFIX, filter);

                using (var projectReader = WebOperation.ExecuteGet<ProjectReader>(
                ctx.Sast.Json.CreateClient
                , (response) =>
                {
                    using (var sr = new StreamReader(response.Content.ReadAsStreamAsync().Result))
                    using (var jtr = new JsonTextReader(sr))
                    {
                        JToken jt = JToken.Load(jtr);

                        return new ProjectReader(jt["value"], ctx, token);
                    }
                }
                , requestUrl
                , ctx.Sast
                , token))
                    fetchedPage = new List<Project>(projectReader);

                if (fetchedPage != null)
                {
                    returnedResults.AddRange(fetchedPage);
                    filter["$filter"] = $"id gt {fetchedPage[fetchedPage.Count - 1].ProjectId}";
                }


            } while (fetchedPage != null && fetchedPage.Count == ODATA_TOP);

            return returnedResults;
        }

        public static IEnumerable<Project> GetProjects(CxSASTRestContext ctx, CancellationToken token, bool useOData)
        {
            if (useOData)
                return GetProjects_odata(ctx, token);
            else
                return GetProjects_rest(ctx, token);
        }

        private static IEnumerable<Project> GetProjects_rest(CxSASTRestContext ctx, CancellationToken token)
        {
            using (var projectReader = WebOperation.ExecuteGet<ProjectReader>(
            ctx.Sast.Json.CreateClient
            , (response) =>
            {
                using (var sr = new StreamReader(response.Content.ReadAsStreamAsync().Result))
                using (var jtr = new JsonTextReader(sr))
                {
                    JToken jt = JToken.Load(jtr);

                    return new ProjectReader(jt, ctx, token);
                }
            }
            , UrlUtils.MakeUrl(ctx.Sast.ApiUrl, REST_URL_SUFFIX)
            , ctx.Sast
            , token, apiVersion: GetApiVersion(ctx, token) ))
                return new List<Project>(projectReader);
        }
    }
}
