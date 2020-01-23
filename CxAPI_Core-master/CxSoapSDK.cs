using System;
using System.Collections.Generic;
using System.ServiceModel.Description;


namespace CxAPI_Core
{
    public class CxSoapSDK : IDisposable
    {
 
       
        CxSDKWebService.Credentials _credentials;
        settingClass _settings;
        resultClass _token;
        bool _debug = false;

        public CxSoapSDK(resultClass token)
        {
            using (secure secure = new secure())
            {
                _settings = secure.get_settings();
            }
            _token = token;
            _debug = token.debug;
        }

        public bool getCredentials()
        {
            string user_name = _token.user_name;
            string credential = _token.credential;

            if ((String.IsNullOrEmpty(user_name)) || (String.IsNullOrEmpty(credential)))
            {
                secure secure = new secure(_token);
                _token = secure.decrypt_Credentials();
                user_name = _token.user_name;
                credential = _token.credential;
                if ((String.IsNullOrEmpty(user_name)) || (String.IsNullOrEmpty(credential)))
                {
                    Console.Error.WriteLine("Credentials not provided or stored, cannot continue.");
                    return false;
                }
            }

            _credentials = new CxSDKWebService.Credentials() { User = user_name, Pass = credential };
            return true;

        }

        public void GetResolverSDKUrl()
        {
            if (getCredentials())
            {
                try
                {
                    CxApiResolver.CxWSResolverSoapClient cxWSResolver = new CxApiResolver.CxWSResolverSoapClient(CxApiResolver.CxWSResolverSoapClient.EndpointConfiguration.CxWSResolverSoap12);
                    CxApiResolver.GetWebServiceUrlResponse response = cxWSResolver.GetWebServiceUrlAsync(CxApiResolver.CxClientType.SDK, 1).Result;
                    if (_debug)
                    {
                        Console.WriteLine("cxWSResolver returns: {0}", response.Body.GetWebServiceUrlResult.ServiceURL);
                        Console.WriteLine("CxSDKWebService using: {0}", _settings.CxSDKWebService);
                    }
                }
                catch
                {
                    Console.Error.WriteLine("cxWSResolver failed to connect to server.");
                }
            }
        }

        public bool LogAdminIn()
        {
            if (String.IsNullOrEmpty(_token.session_id))
            {
                GetResolverSDKUrl();
                CxSDKWebService.CxSDKWebServiceSoapClient cxSDKWebServiceSoapClient = new CxSDKWebService.CxSDKWebServiceSoapClient(CxSDKWebService.CxSDKWebServiceSoapClient.EndpointConfiguration.CxSDKWebServiceSoap12);
                CxSDKWebService.LoginResponse cxWSResponseLoginData = cxSDKWebServiceSoapClient.LoginAsync(_credentials, 1033).Result;
                _token.session_id = cxWSResponseLoginData.Body.LoginResult.SessionId;
                if (String.IsNullOrEmpty(_token.session_id))
                {
                    Console.Error.WriteLine("SOAP login failed, please try again with different credentials");
                    return false;
                }
            }
            return true;
        }

        public resultClass makeScanCsv(resultClass token)
        {
            token.status = -1;
           
            if ((token.start_time == null ) || (token.end_time == null))
            {
                Console.Error.WriteLine("Start time and End time must be provided.");
                return token;
            }
            if (LogAdminIn())
            {
                CxSDKWebService.CxSDKWebServiceSoapClient cxSDKProxy = new CxSDKWebService.CxSDKWebServiceSoapClient(CxSDKWebService.CxSDKWebServiceSoapClient.EndpointConfiguration.CxSDKWebServiceSoap12);
                Dictionary<long,string> presets = GetPresetConfiguration(cxSDKProxy);
                Dictionary<long, CxSDKWebService.ProjectDisplayData> projects = GetAllProjects(cxSDKProxy);

                foreach (KeyValuePair<long, CxSDKWebService.ProjectDisplayData> project in projects)
                {
                    CxSDKWebService.GetProjectConfigurationResponse response = GetProjectConfiguration(cxSDKProxy, token, project.Value.projectID);
                    csvScanOutput_1 csv = new csvScanOutput_1();
                 
                }
                token.status = 0;
            }
            return token;
        }

        public resultClass makeProjectScanCsv_1()
        {
            _token.status = -1;
            csvHelper cvsHelper = new csvHelper();
           

            if ((_token.start_time == null))
            {
                Console.Error.WriteLine("Start time must be provided.");
                return _token;
            }
            if (LogAdminIn())
            {
                CxSDKWebService.CxSDKWebServiceSoapClient cxSDKProxy = new CxSDKWebService.CxSDKWebServiceSoapClient(CxSDKWebService.CxSDKWebServiceSoapClient.EndpointConfiguration.CxSDKWebServiceSoap12);
                Dictionary<long, string> presets = GetPresetConfiguration(cxSDKProxy);
                Dictionary<long, CxSDKWebService.ProjectDisplayData> projects = GetAllProjects(cxSDKProxy);
                List<csvScanOutput_1> csvOutput = new List<csvScanOutput_1>();
                List<CxSDKWebService.ProjectScannedDisplayData> scannedDisplayDatasList = GetProjectScannedList(cxSDKProxy);
                foreach (CxSDKWebService.ProjectScannedDisplayData scans in scannedDisplayDatasList)
                {
                    CxSDKWebService.ProjectDisplayData proj = projects[scans.ProjectID];
                    CxSDKWebService.CxDateTime scanDate = proj.LastScanDate;

                    DateTime lastScanDate = DateTime.Parse(String.Format("{0}/{1}/{2} {3}:{4}:{5}", scanDate.Month, scanDate.Day, scanDate.Year, scanDate.Hour, scanDate.Minute, scanDate.Second));
                    if (lastScanDate > _token.start_time)
                    {
                        if ((_token.end_time == null) || (lastScanDate < _token.end_time))
                        {
                            if ((_token.project_name == null) || (scans.ProjectName.Contains(_token.project_name)))
                            {
                                csvScanOutput_1 csv = new csvScanOutput_1()
                                {
                                    Project_Name = scans.ProjectName,
                                    Owner = proj.Owner,
                                    Team = scans.TeamName,
                                    Preset = proj.Preset,
                                    Last_Scan = lastScanDate,
                                    Total_Vulerabilities = scans.TotalVulnerabilities,
                                    High = scans.HighVulnerabilities,
                                    Medium = scans.MediumVulnerabilities,
                                    Low = scans.LowVulnerabilities,
                                    Info = scans.InfoVulnerabilities
                                };
                                csvOutput.Add(csv);
                                if ((_token.pipe) || (_token.debug))
                                {
                                    Console.WriteLine("{0},{1},{2},{3},{4},{5},{6},{7},{8}", csv.Project_Name, csv.Owner, csv.Team, csv.Preset, csv.Last_Scan, csv.Total_Vulerabilities, csv.High, csv.Medium, csv.Low, csv.Info);
                                }
                            }

                        }

                    }
                }
                if (!_token.pipe)
                {
                    _token.status = cvsHelper.writeCVSFile(csvOutput, _token);
                }
            }
            return _token;
        }
        public resultClass makeProjectScanCsv_2()
        {
            _token.status = -1;
            csvHelper cvsHelper = new csvHelper();


            if ((_token.start_time == null))
            {
                Console.Error.WriteLine("Start time must be provided.");
                return _token;
            }
            if (LogAdminIn())
            {
                CxSDKWebService.CxSDKWebServiceSoapClient cxSDKProxy = new CxSDKWebService.CxSDKWebServiceSoapClient(CxSDKWebService.CxSDKWebServiceSoapClient.EndpointConfiguration.CxSDKWebServiceSoap12);
                Dictionary<long, string> presets = GetPresetConfiguration(cxSDKProxy);
                Dictionary<long, CxSDKWebService.ProjectDisplayData> projects = GetAllProjects(cxSDKProxy);
                Dictionary<long, DateTime?> origin = GetAllScans(cxSDKProxy);
                List<csvScanOutput_2> csvOutput = new List<csvScanOutput_2>();
                List<CxSDKWebService.ProjectScannedDisplayData> scannedDisplayDatasList = GetProjectScannedList(cxSDKProxy);

                foreach (CxSDKWebService.ProjectScannedDisplayData scans in scannedDisplayDatasList)
                {
                    CxSDKWebService.ProjectDisplayData proj = projects[scans.ProjectID];
                    CxSDKWebService.CxDateTime scanDate = proj.LastScanDate;
 
                    DateTime lastScanDate = DateTime.Parse(String.Format("{0}/{1}/{2} {3}:{4}:{5}", scanDate.Month, scanDate.Day, scanDate.Year, scanDate.Hour, scanDate.Minute, scanDate.Second));
                    if (lastScanDate > _token.start_time)

                    {
                        if ((_token.end_time == null) || (lastScanDate < _token.end_time))
                        {
                            if ((_token.project_name == null) || (scans.ProjectName.Contains(_token.project_name)))
                            {
                                csvScanOutput_2 csv = new csvScanOutput_2()
                                {
                                    Project_Name = scans.ProjectName,
                                    Owner = proj.Owner,
                                    Team = scans.TeamName,
                                    Onboarded = "Onboarded",
                                    Onboarding_Date = (DateTime)origin[scans.ProjectID],
                                    Last_Scan = lastScanDate,
                                    Total_Vulerabilities = scans.TotalVulnerabilities,
                                    High = scans.HighVulnerabilities,
                                    Medium = scans.MediumVulnerabilities
                                };
                                csvOutput.Add(csv);
                                if ((_token.pipe) || (_token.debug))
                                {
                                    Console.WriteLine("{0},{1},{2},{3},{4},{5},{6},{7},{8}", csv.Team, csv.Project_Name, csv.Owner, "Onboarded", csv.Onboarding_Date, csv.Last_Scan, csv.Total_Vulerabilities, csv.High, csv.Medium);
                                }
                            }

                        }

                    }
                }
                if (!_token.pipe)
                {
                    _token.status = cvsHelper.writeCVSFile(csvOutput, _token);
                }
            }
            return _token;
        }
        public List<CxSDKWebService.ProjectScannedDisplayData> GetProjectScannedList(CxSDKWebService.CxSDKWebServiceSoapClient cxSDKProxy)
        {
            if (LogAdminIn())
            {
                if (cxSDKProxy == null)
                {
                    cxSDKProxy = new CxSDKWebService.CxSDKWebServiceSoapClient(CxSDKWebService.CxSDKWebServiceSoapClient.EndpointConfiguration.CxSDKWebServiceSoap12);
                }
                CxSDKWebService.GetProjectScannedDisplayDataResponse cxWSResponseProject = cxSDKProxy.GetProjectScannedDisplayDataAsync(_token.session_id).Result;
                List<CxSDKWebService.ProjectScannedDisplayData> projectScannedDisplayDatas = new List<CxSDKWebService.ProjectScannedDisplayData>(cxWSResponseProject.Body.GetProjectScannedDisplayDataResult.ProjectScannedList);

                return projectScannedDisplayDatas;
            }
            return null;
        }
        public CxSDKWebService.GetProjectConfigurationResponse GetProjectConfiguration(CxSDKWebService.CxSDKWebServiceSoapClient cxSDKProxy, resultClass token,long project_id)
        {
            if (LogAdminIn())
            {   if (cxSDKProxy == null)
                {
                    cxSDKProxy = new CxSDKWebService.CxSDKWebServiceSoapClient(CxSDKWebService.CxSDKWebServiceSoapClient.EndpointConfiguration.CxSDKWebServiceSoap12); ;
                }
                CxSDKWebService.GetProjectConfigurationResponse projectConfigurationResponse = cxSDKProxy.GetProjectConfigurationAsync(token.session_id, project_id).Result;
                return projectConfigurationResponse;
            }
            return null;
        }
        public Dictionary<long,string> GetPresetConfiguration(CxSDKWebService.CxSDKWebServiceSoapClient cxSDKProxy)
        {
            Dictionary<long, string> presets = new Dictionary<long, string>();
            if (LogAdminIn())
            {
                if (cxSDKProxy == null)
                {
                    cxSDKProxy = new CxSDKWebService.CxSDKWebServiceSoapClient(CxSDKWebService.CxSDKWebServiceSoapClient.EndpointConfiguration.CxSDKWebServiceSoap12); ;
                }
                CxSDKWebService.GetPresetListResponse cxWSResponsePresetList = cxSDKProxy.GetPresetListAsync(_token.session_id).Result;
                
                List<CxSDKWebService.Preset> getPresetListResponses = new List<CxSDKWebService.Preset>(cxWSResponsePresetList.Body.GetPresetListResult.PresetList);
                foreach(CxSDKWebService.Preset preset in getPresetListResponses)
                {
                    presets.Add(preset.ID, preset.PresetName);
                }
                return presets;
            }
           return null;
        }
    
        public Dictionary<long, CxSDKWebService.ProjectDisplayData> GetAllProjects(CxSDKWebService.CxSDKWebServiceSoapClient cxSDKProxy)
        {
            Dictionary<long, CxSDKWebService.ProjectDisplayData> projects = new Dictionary<long, CxSDKWebService.ProjectDisplayData>();
            if (LogAdminIn())
            {
                if (cxSDKProxy == null)
                {
                    cxSDKProxy = new CxSDKWebService.CxSDKWebServiceSoapClient(CxSDKWebService.CxSDKWebServiceSoapClient.EndpointConfiguration.CxSDKWebServiceSoap12); ;
                }
                CxSDKWebService.GetProjectsDisplayDataResponse cxWSResponseProjects = cxSDKProxy.GetProjectsDisplayDataAsync(_token.session_id).Result;

                List<CxSDKWebService.ProjectDisplayData> getProjectListResponses = new List<CxSDKWebService.ProjectDisplayData>(cxWSResponseProjects.Body.GetProjectsDisplayDataResult.projectList);
                foreach (CxSDKWebService.ProjectDisplayData project in getProjectListResponses)
                {
                   projects.Add(project.projectID,project);
                }
                return projects;
            }
            return null;
        }

        public Dictionary<long, DateTime?> GetAllScans(CxSDKWebService.CxSDKWebServiceSoapClient cxSDKProxy)
        {
            Dictionary<long, DateTime?> origin = new Dictionary<long, DateTime?>();
            if (LogAdminIn())
            {
                if (cxSDKProxy == null)
                {
                    cxSDKProxy = new CxSDKWebService.CxSDKWebServiceSoapClient(CxSDKWebService.CxSDKWebServiceSoapClient.EndpointConfiguration.CxSDKWebServiceSoap12); ;
                }
                CxSDKWebService.GetScansDisplayDataForAllProjectsResponse getProjecScannedDisplay = cxSDKProxy.GetScansDisplayDataForAllProjectsAsync(_token.session_id).Result;
                List<CxSDKWebService.ScanDisplayData> scans = new List<CxSDKWebService.ScanDisplayData>(getProjecScannedDisplay.Body.GetScansDisplayDataForAllProjectsResult.ScanList);


                foreach (CxSDKWebService.ScanDisplayData scan  in scans)
                {
                    CxSDKWebService.CxDateTime scanDate = scan.QueuedDateTime;
                    DateTime firstScanDate = DateTime.Parse(String.Format("{0}/{1}/{2} {3}:{4}:{5}", scanDate.Month, scanDate.Day, scanDate.Year, scanDate.Hour, scanDate.Minute, scanDate.Second));
                    if (!origin.ContainsKey(scan.ProjectId))
                    {
                        origin.Add(scan.ProjectId, firstScanDate);
                    }
                    else
                    {
                        if (origin[scan.ProjectId] > firstScanDate)
                        {
                            origin[scan.ProjectId] = firstScanDate;
                        }
                    }
                }
                return origin;
            }
            return null;
        }

        public void Dispose()
        {
            Dispose(true);
        }

        protected virtual void Dispose(bool disposing)
        {

        }
    }

}

