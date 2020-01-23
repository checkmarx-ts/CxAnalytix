using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using CxAPI_Core.dto;

namespace CxAPI_Core
{
    partial class CxRest_API : IDisposable
    {
        public resultClass token;

        public CxRest_API(resultClass token)
        {
            this.token = token;
        }

        private bool getlastReport(XElement result, Dictionary<long, ReportStaging> end, Dictionary<long, List<ReportResultNew>> last)
        {
            foreach (long key in end.Keys)
            {
                ReportStaging staging = end[key];
                if (result.Attribute("ScanId").Value == staging.ScanId.ToString())
                {
                    last.Add(staging.ProjectId, process_LastScan(result, staging.ScanId));
                }
            }
            return true;
        }

        private void setCount(long id, Dictionary<long, ScanCount> scanCount)
        {
            if (scanCount.ContainsKey(id))
            {
                ScanCount sc = scanCount[id];
                sc.count++;
                scanCount[id] = sc;
            }
            else
            {
                ScanCount sc = new ScanCount();
                sc.count = 1;
                scanCount.Add(id, sc);
            }
        }

        private List<ReportOutput> totalScansandReports(Dictionary<long, ReportStaging> start, Dictionary<long, ReportStaging> end, List<ReportResultNew> resultNew, Dictionary<long, List<ReportResultNew>> lastScan, Dictionary<long, ScanCount> scanCount)
        {
            List<ReportOutput> reports = new List<ReportOutput>();
            foreach (long key in start.Keys)
            {
                ReportOutput report = new ReportOutput();

                ReportStaging first = start[key];
                ReportStaging last = end[key];
                List<ReportResultNew> lastScanResults = lastScan[key];
                foreach (ReportResultNew result in resultNew)
                {
                    if (result.projectId == first.ProjectId)
                    {
                        if (result.status == "New")
                        {
                            if (result.Severity == "High") { report.NewHigh++; }
                            else if (result.Severity == "Medium") { report.NewMedium++; }
                            else if (result.Severity == "Low") { report.NewLow++; }
                        }
                    }
                }
                foreach (ReportResultNew result in lastScanResults)
                {
                    if (result.state == 0) { report.ToVerify++; }
                    else if (result.state == 1) { report.NotExploitable++; }
                    else if (result.state == 2) { report.Confirmed++; }
                }

                report.ProjectName = first.ProjectName;
                report.StartHigh = first.High;
                report.StartMedium = first.Medium;
                report.StartLow = first.Low;
                report.firstScan = first.dateTime;

                report.LastHigh = last.High;
                report.LastMedium = last.Medium;
                report.LastLow = last.Low;
                report.lastScan = last.dateTime;

                report.DiffHigh = first.High - last.High;
                report.DiffMedium = first.Medium - last.Medium;
                report.DiffLow = first.Low - last.Low;
                report.ScanCount = scanCount[key].count;
                reports.Add(report);

            }
            return reports;
        }

        private bool process_CxResponse(XElement result, List<ReportResultNew> response)
        {
            try
            {
                IEnumerable<XElement> newVulerability = from el in result.Descendants("Query").Descendants("Result")
                                                        where (string)el.Attribute("Status").Value == "New"
                                                        select el;

                foreach (XElement el in newVulerability)
                {
                    XElement query = el.Parent;
                    XElement root = query.Parent;
                    ReportResultNew isnew = new ReportResultNew()
                    {
                        Query = query.Attribute("name").Value.ToString(),
                        Group = query.Attribute("group").Value.ToString(),
                        projectId = Convert.ToInt64(root.Attribute("ProjectId").Value.ToString()),
                        scanId = Convert.ToInt64(root.Attribute("ScanId").Value.ToString()),
                        status = el.Attribute("Status").Value.ToString(),
                        Severity = el.Attribute("Severity").Value.ToString(),
                        state = Convert.ToInt32(el.Attribute("state").Value.ToString())
                    };
                    response.Add(isnew);

                }
                return true;
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine(ex.Message);
                return false;
            }

        }
        private List<ReportResultNew> process_LastScan(XElement result, long scanId)
        {
            List<ReportResultNew> reportResults = new List<ReportResultNew>();
            try
            {
                if (result.Attribute("ScanId").Value == scanId.ToString())
                {
                    IEnumerable<XElement> lastScan = from el in result.Descendants("Query").Descendants("Result")
                                                     select el;
                    foreach (XElement el in lastScan)
                    {
                        XElement query = el.Parent;
                        XElement root = query.Parent;
                        ReportResultNew isnew = new ReportResultNew()
                        {
                            Query = query.Attribute("name").Value.ToString(),
                            Group = query.Attribute("group").Value.ToString(),
                            projectId = Convert.ToInt64(root.Attribute("ProjectId").Value.ToString()),
                            scanId = Convert.ToInt64(root.Attribute("ScanId").Value.ToString()),
                            status = el.Attribute("Status").Value.ToString(),
                            Severity = el.Attribute("Severity").Value.ToString(),
                            state = Convert.ToInt32(el.Attribute("state").Value.ToString())
                        };

                        reportResults.Add(isnew);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine(ex.Message);
            }

            return reportResults;

        }
        private bool findFirstorLastScan(long projectId, ScanObject scan, Dictionary<long, ReportStaging> keyValuePairs, bool operation)
        {
            getScans scans = new getScans();

            if (keyValuePairs.ContainsKey(scan.Project.Id))
            {

                bool start = false;
                ReportStaging reportStaging = keyValuePairs[scan.Project.Id];
                long diff = DateTimeOffset.Compare(reportStaging.dateTime, (DateTimeOffset)scan.DateAndTime.StartedOn);
                if (operation)
                {
                    start = (diff > 0) ? true : false;
                }
                else
                {
                    start = (diff < 0) ? true : false;
                }
                if (start)
                {
                    ScanStatistics scanStatistics = scans.getScansStatistics(scan.Id, token);
                    ReportStaging staging = new ReportStaging()
                    {
                        ProjectId = scan.Project.Id,
                        ProjectName = scan.Project.Name,
                        dateTime = (DateTimeOffset)scan.DateAndTime.StartedOn,
                        High = scanStatistics.HighSeverity,
                        Medium = scanStatistics.MediumSeverity,
                        Low = scanStatistics.LowSeverity,
                        ScanId = scan.Id
                    };
                    keyValuePairs[scan.Project.Id] = staging;
                }
            }
            else
            {
                ScanStatistics scanStatistics = scans.getScansStatistics(scan.Id, token);
                keyValuePairs.Add(scan.Project.Id, new ReportStaging()
                {
                    ProjectId = scan.Project.Id,
                    ProjectName = scan.Project.Name,
                    dateTime = (DateTimeOffset)scan.DateAndTime.StartedOn,
                    High = scanStatistics.HighSeverity,
                    Medium = scanStatistics.MediumSeverity,
                    Low = scanStatistics.LowSeverity,
                    ScanId = scan.Id
                });
            }
            return true;
        }

        public void Dispose()
        {

        }

    }

}
