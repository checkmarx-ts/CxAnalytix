using CxRestClient.Utility;
using log4net;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace CxRestClient.SCA
{
    public class CxProjects
    {
        private static ILog _log = LogManager.GetLogger(typeof(CxProjects));

        private static String URL_SUFFIX = "risk-management/projects";

        private CxProjects()
        { }



        public static String GetProjects(CxSCARestContext ctx, CancellationToken token)
        {

            return WebOperation.ExecuteGet<String>(ctx.Json.CreateClient, (response) => { 
                var reader = new StreamReader(response.Content.ReadAsStreamAsync().Result);

                return reader.ReadToEnd();
            
            }, 
                UrlUtils.MakeUrl(ctx.ApiUrl, URL_SUFFIX), ctx, token);


            //using (var projectReader = WebOperation.ExecuteGet<ProjectReader>(
            //ctx.Json.CreateClient
            //, (response) =>
            //{
            //    using (var sr = new StreamReader(response.Content.ReadAsStreamAsync().Result))
            //    using (var jtr = new JsonTextReader(sr))
            //    {
            //        JToken jt = JToken.Load(jtr);

            //        return new ProjectReader(jt, ctx, token);
            //    }
            //}
            //, UrlUtils.MakeUrl(ctx.ApiUrl, URL_SUFFIX)
            //, ctx
            //, token, apiVersion: "2.0"))
            //    return new List<Project>(projectReader);
        }

    }
}
