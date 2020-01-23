using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CxAPI_Core.dto;

namespace CxAPI_Core
{
    public class tokenClass
    {
        public string user_name { get; set; }
        public string credential { get; set; }
        public string grant_type { get; set; }
        public string scope { get; set; }
        public string client_id { get; set; }
        public string client_secret { get; set; }
        public string CxUrl { get; set; }
        public string CxAPIResolver { get; set; }
        public string CxSDKWebService { get; set; }
        public string get_token { get; set; }
        public string bearer_token { get; set; }
        public double expiration { get; set; }
        public DateTime timestamp { get; set; }


    }

    public class resultClass : tokenClass
    {
        public int status { get; set; }
        public string statusMessage { get; set; }
        public string request { get; set; }
        public api_action api_action { get; set; }
        public string file_path { get; set; }
        public string file_name { get; set; }
        public string save_result_path { get; set; }
        public string save_result_filename { get; set; }
        public string save_result { get; set; }
        public string op_result { get; set; }
        public byte[] byte_result { get; set; }
        public DateTime? start_time { get; set; }
        public DateTime? end_time { get; set; }
        public string session_id { get; set; }
        public string project_name { get; set; }
        public bool pipe { get; set; }
        public string os_path { get; set; }
        public bool debug { get; set; }
        public int verbosity{ get; set; }
        public string report_name { get; set; }

        List<ProjectObject> projectClass { get; set; }

        public resultClass()
        {
            secure secure = new secure();
            settingClass settings = secure.get_settings();

            grant_type = settings.grant_type;
            scope = settings.scope;
            client_id = settings.client_id;
            client_secret = settings.client_secret;
            CxUrl = settings.CxUrl;
            file_name = settings.CxDefaultFileName;
            file_path = settings.CxFilePath;
            timestamp = DateTime.UtcNow;
            start_time = null;
            end_time = null;
            api_action = api_action.help;
            os_path = secure._os.Contains("Windows") ? "\\" : "/";
            debug = false;
            CxAPIResolver = CxUrl + settings.CxAPIResolver;
            CxSDKWebService = CxUrl + settings.CxSDKWebService;
          
        }

    }

    public class resultToken
    {
        public string access_token { get; set; }
        public string expires_in { get; set; }
        public string token_type { get; set; }
    }

    public class encryptClass
    {
        public string user_name { get; set; }
        public string credential { get; set; }
        public string token { get; set; }
        public string token_creation { get; set; }
        public string token_expires { get; set; }
    }
    public class settingClass
    {
        public string CxUrl { get; set; }
        public string CxAPIResolver { get; set; }
        public string CxSDKWebService { get; set; }
        public string CxFilePath { get; set; }
        public string CxDefaultFileName { get; set; }
        public string CxDataFilePath { get; set; }
        public string CxDataFileName { get; set; }
        public string CxResultFileName { get; set; }
        public string CxResultFilePath { get; set; }
        public string grant_type { get; set; }
        public string scope { get; set; }
        public string client_id { get; set; }
        public string client_secret { get; set; }
        public string token { get; set; }
        public string project { get; set; }
        public string scans { get; set; }
        public string debug { get; set; }
        
    }
    public class settingToken
    {
        public string CxUrl { get; set; }
        public string action { get; set; }
    }
    public class csvScanOutput_1
    {
        public string Project_Name { get; set; }
        public string Owner { get; set; }
        public string Team { get; set; }
        public string Preset { get; set; }
        public int Total_Vulerabilities { get; set; }
        public DateTime Last_Scan { get; set; }
        public int High { get; set; }
        public int Medium { get; set; }
        public int Low { get; set; }
        public int Info { get; set; }

    }
    public class csvScanOutput_2
    {
        public string Team { get; set; }
        public string Project_Name { get; set; }
        public string Owner { get; set; }
        public string Onboarded { get; set; }
        public DateTime Onboarding_Date { get; set; }
        public int Total_Vulerabilities { get; set; }
        public DateTime Last_Scan { get; set; }
        public int High { get; set; }
        public int Medium { get; set; }
     
    }

}


