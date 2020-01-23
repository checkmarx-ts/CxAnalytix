using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CxAPI_Core
{
    class dispatcher
    {
        public resultClass dispatch()
        {
            resultClass token = Configuration.mono_command_args();
            if (token.status != 0) { return token; }
            secure secure = new secure(token);
            _options.debug = token.debug;
            _options.level = token.verbosity;

            switch (token.api_action)
            {
                case api_action.getToken:
                    {
                        fetchToken newtoken = new fetchToken();
                        newtoken.get_token(secure.decrypt_Credentials());
                        break;
                    }
                case api_action.storeCredentials:
                    {
                        storeCredentials cred = new storeCredentials();
                        token = cred.save_credentials(token);
                        break;
                    }
                case api_action.scanResults:
                    {
                        if (token.report_name.Contains("REST_REPORT_1"))
                        {
                            using (restReport_1 restReport = new restReport_1(token))
                            {
                                if (token.report_name == "REST_REPORT_1")
                                {
                                    restReport.fetchReportsbyDate();
                                }
                            }
                        }
                        else if (token.report_name.Contains("REST_REPORT_2"))
                        {
                            using (restReport_2 restReport = new restReport_2(token))
                            {
                                if (token.report_name == "REST_REPORT_2")
                                {
                                    restReport.fetchReportsbyDate();
                                }
                            }
                        }
                        else if (token.report_name.Contains("REST_REPORT_3"))
                        {
                            using (restReport_3 restReport = new restReport_3(token))
                            {
                                if (token.report_name == "REST_REPORT_3")
                                {
                                    restReport.fetchReportsbyDate();
                                }
                            }
                        }
                        else if (token.report_name.Contains("REST_STORE_1"))
                        {
                            using (restStore_1 restReport = new restStore_1(token))
                            {
                                if (token.report_name == "REST_STORE_1")
                                {
                                    restReport.fetchResultsbyDate();
                                }
                            }
                        }
                        using (CxSoapSDK cxSoapSDK = new CxSoapSDK(token))
                        {
                            if (token.report_name == "REPORT_1")
                            {
                                cxSoapSDK.makeProjectScanCsv_1();
                            }
                            if (token.report_name == "REPORT_2")
                            {
                                cxSoapSDK.makeProjectScanCsv_2();
                            }
                        }
          
                        break;
                    }
                case api_action.getProjects:
                    {
                        getProjects getProjects = new getProjects();
                        getProjects.get_projects(token);
                        break;
                    }

            }
            return token;
        }
    }
}
