using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using System.Collections.Generic;
using CxAPI_Core.dto;

namespace CxAPI_Core
{
    class getProjects
    {
        public List<ProjectObject> get_projects(resultClass token)
        {
            get httpGet = new get();
            List<ProjectObject> pclass = new List<ProjectObject>();
            secure token_secure = new secure(token);
            token_secure.findToken(token);
            string path= token_secure.get_rest_Uri(CxConstant.CxAllProjects) ;
            httpGet.get_Http(token, path);
            if (token.status == 0)
            {
                pclass =  JsonConvert.DeserializeObject<List<ProjectObject>>(token.op_result);
            }

            return pclass;
        }
    }
}
