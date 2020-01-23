using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using CxAPI_Core.dto;

namespace CxAPI_Core
{
    class restReport_1 : IDisposable
    {
        public resultClass token;

        public restReport_1(resultClass token)
        {
            this.token = token;
        }

        public bool fetchReportsbyDate()
        {
            List<ReportTrace> trace = new List<ReportTrace>();
            List<ReportResultNew> resultNew = new List<ReportResultNew>();
            Dictionary<long, ReportStaging> start = new Dictionary<long, ReportStaging>();
            Dictionary<long, ReportStaging> end = new Dictionary<long, ReportStaging>();
            Dictionary<long, List<ReportResultNew>> last = new Dictionary<long, List<ReportResultNew>>();
            Dictionary<long, ScanCount> scanCount = new Dictionary<long, ScanCount>();
            bool waitFlag = false;
            getScanResults scanResults = new getScanResults();
            getScans scans = new getScans();
            List<ScanObject> scan = scans.getScan(token);
            foreach (ScanObject s in scan)
            {
                if ((s.DateAndTime != null) && (s.Status.Id == 7) && (s.DateAndTime.StartedOn > token.start_time) && (s.DateAndTime.StartedOn < token.end_time))
                {
                    setCount(s.Project.Id, scanCount);
                    findFirstorLastScan(s.Project.Id, s, start, true);
                    findFirstorLastScan(s.Project.Id, s, end, false);

                    ReportResult result = scanResults.SetResultRequest(s.Id, "XML", token);
                    if (result != null)
                    {
                        trace.Add(new ReportTrace(s.Project.Id, s.Project.Name, s.DateAndTime.StartedOn, s.Id, result.ReportId,"XML"));
                    }
                }
            }
            while (!waitFlag)
            {
                foreach (ReportTrace rt in trace)
                {
                    waitFlag = true;
                    if (!rt.isRead)
                    {
                        waitFlag = false;
                        if (scanResults.GetResultStatus(rt.reportId, token))
                        {
                            var result = scanResults.GetResult(rt.reportId, token);
                            if (result != null)
                            {
                                if (process_CxResponse(result, resultNew))
                                {
                                    rt.isRead = true;
                                    getlastReport(result, end, last);
                                }
                            }
                        }
                    }
                }
            }

            List<ReportOutput> reportOutputs = totalScansandReports(start, end, resultNew, last, scanCount);
            if (token.pipe)
            {
                foreach (ReportOutput csv in reportOutputs)
                {
                    Console.WriteLine("{0},{1},{2},{3},{4},{5},{6},{7},{8},{9},{10},{11},{12},{13},{14},{15}", csv.ProjectName, csv.LastHigh, csv.LastMedium, csv.LastLow, csv.NewHigh, csv.NewMedium, csv.NewLow, csv.DiffHigh, csv.DiffMedium, csv.DiffLow, csv.NotExploitable, csv.Confirmed, csv.ToVerify, csv.firstScan, csv.lastScan, csv.ScanCount);
                }
            }
            else
            {
                csvHelper csvHelper = new csvHelper();
                csvHelper.writeCVSFile(reportOutputs, token);
            }
            return true;
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