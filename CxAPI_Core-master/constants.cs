using System;
using System.Collections.Generic;
using System.Text;

namespace CxAPI_Core
{
    static class CxConstant
    {
        public const string CxToken = "/cxrestapi/auth/identity/connect/token";
        public const string CxAllProjects = "/cxrestapi/projects";
        public const string CxProject = "/cxrestapi/projects/{0}";
        public const string CxScans = "/cxrestapi/sast/scans";
        public const string CxReportRegister = "/cxrestapi/reports/sastScan";
        public const string CxReportFetch = "/cxrestapi/reports/sastScan/{0}";
        public const string CxReportStatus = "/cxrestapi/reports/sastScan/{0}/status";
        public const string CxScanStatistics = "/cxrestapi/sast/scans/{0}/resultsStatistics";
    }
    static class _options
    {
        public static bool debug;
        public static int level;
    }

}
