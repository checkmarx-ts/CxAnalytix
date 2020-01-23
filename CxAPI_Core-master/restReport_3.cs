using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Xml.Linq;
using CxAPI_Core.dto;
using System.Threading;

namespace CxAPI_Core
{
    class restReport_3 : IDisposable
    {
        public resultClass token;

        public restReport_3(resultClass token)
        {
            this.token = token;
        }

        public bool fetchReportsbyDate()
        {
            Dictionary<long, Dictionary<DateTime, Dictionary<string, ReportResultExtended>>> fix = new Dictionary<long, Dictionary<DateTime, Dictionary<string, ReportResultExtended>>>();
            List<ReportTrace> trace = new List<ReportTrace>();
            Dictionary<string, ReportResultExtended> resultAll = new Dictionary<string, ReportResultExtended>();
            List<ReportResultExtended> report_output = new List<ReportResultExtended>();
            //            Dictionary<long, ReportStaging> start = new Dictionary<long, ReportStaging>();
            //            Dictionary<long, ReportStaging> end = new Dictionary<long, ReportStaging>();
            Dictionary<long, ScanCount> scanCount = new Dictionary<long, ScanCount>();
            ConsoleSpinner spinner = new ConsoleSpinner();
            bool waitFlag = false;
            getScanResults scanResults = new getScanResults();
            getScans scans = new getScans();
            List<ScanObject> scan = scans.getScan(token);
            foreach (ScanObject s in scan)
            {
                if ((s.DateAndTime != null) && (s.Status.Id == 7) && (s.DateAndTime.StartedOn > token.start_time) && (s.DateAndTime.StartedOn < token.end_time))
                {
                    if ((String.IsNullOrEmpty(token.project_name) || ((!String.IsNullOrEmpty(token.project_name)) && (s.Project.Name.Contains(token.project_name)))))
                    {
                        setCount(s.Project.Id, scanCount);

                        ReportResult result = scanResults.SetResultRequest(s.Id, "XML", token);
                        if (result != null)
                        {
                            trace.Add(new ReportTrace(s.Project.Id, s.Project.Name, s.DateAndTime.StartedOn, s.Id, result.ReportId, "XML"));
                        }

                    }
                }
            }
            while (!waitFlag)
            {
                spinner.Turn();
                waitFlag = true;
                if (token.debug && token.verbosity > 0) { Console.WriteLine("Sleeping 1 second(s)"); }
                Thread.Sleep(1000);
                foreach (ReportTrace rt in trace)
                {
                    if (!rt.isRead)
                    {
                        waitFlag = false;
                        if (token.debug && token.verbosity > 0) { Console.WriteLine("Testing report.Id {0}", rt.reportId); }
                        if (scanResults.GetResultStatus(rt.reportId, token))
                        {
                            if (token.debug && token.verbosity > 0) { Console.WriteLine("Found report.Id {0}", rt.reportId); }
                            var result = scanResults.GetResult(rt.reportId, token);
                            if (result != null)
                            {
                                if (process_CxResponse(rt.reportId, result, resultAll, fix, report_output))
                                {
                                    rt.isRead = true;
                                }
                            }
                        }
                    }
                }
            }
            addFixed(fix, report_output);
            if (token.pipe)
            {
                foreach (ReportResultExtended csv in report_output)
                {
                    Console.WriteLine("{0},{1},{2},{3},{4},{5},{6},{7},{8},{9},{10},{11}", csv.projectName, csv.teamName, csv.presetName, csv.similarityId, csv.resultId, csv.reportId, csv.Severity, csv.status, csv.state, csv.Query, csv.Group, csv.scanDate);
                }
            }
            else
            {
                csvHelper csvHelper = new csvHelper();
                csvHelper.writeCVSFile(report_output, token);
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

        private bool process_CxResponse(long report_id, XElement result, Dictionary<string, ReportResultExtended> response, Dictionary<long, Dictionary<DateTime, Dictionary<string, ReportResultExtended>>> fix, List<ReportResultExtended> report_output)
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
                    XElement path = el.Descendants("Path").FirstOrDefault();
                    XElement pathNode = path.Descendants("PathNode").FirstOrDefault();
                    XElement snippet = pathNode.Descendants("Snippet").FirstOrDefault();
                    XElement line = snippet.Descendants("Line").FirstOrDefault();

                    long SimilarityId = Convert.ToInt64(path.Attribute("SimilarityId").Value.ToString());
                    string key = "New-" + SimilarityId.ToString();
                    ReportResultExtended resultExtended = response.GetValueOrDefault(key);
                    if (resultExtended == null)
                    {

                        ReportResultExtended isnew = new ReportResultExtended()
                        {
                            Query = query.Attribute("name").Value.ToString(),
                            Group = query.Attribute("group").Value.ToString(),
                            projectName = root.Attribute("ProjectName").Value.ToString(),
                            presetName = root.Attribute("Preset").Value.ToString(),
                            teamName = root.Attribute("Team").Value.ToString(),
                            scanDate = Convert.ToDateTime(root.Attribute("ScanStart").Value.ToString()),
                            projectId = Convert.ToInt64(root.Attribute("ProjectId").Value.ToString()),
                            resultId = Convert.ToInt64(path.Attribute("ResultId").Value.ToString()),
                            reportId = report_id,
                            nodeId = Convert.ToInt64(el.Attribute("NodeId").Value.ToString()),
                            scanId = Convert.ToInt64(root.Attribute("ScanId").Value.ToString()),
                            status = el.Attribute("Status").Value.ToString(),
                            Severity = el.Attribute("Severity").Value.ToString(),
                            similarityId = Convert.ToInt64(path.Attribute("SimilarityId").Value.ToString()),
                            pathId = Convert.ToInt64(path.Attribute("PathId").Value.ToString()),
                            state = Convert.ToInt32(el.Attribute("state").Value.ToString()),
                            fileName = el.Attribute("FileName").Value.ToString(),
                            lineNo = Convert.ToInt32(el.Attribute("Line").Value.ToString()),
                            column = Convert.ToInt32(el.Attribute("Column").Value.ToString()),
                            firstLine = line.Descendants("Code").FirstOrDefault().Value.ToString(),
                            queryId = Convert.ToInt64(query.Attribute("id").Value.ToString())
                        };
                        response.Add(key, isnew);
                        report_output.Add(isnew);
                    }

                }
                IEnumerable<XElement> recurringVulerability = from el in result.Descendants("Query").Descendants("Result")
                                                              where (string)el.Attribute("Status").Value == "Recurrent"
                                                              select el;
                foreach (XElement el in recurringVulerability)
                {
                    XElement query = el.Parent;
                    XElement root = query.Parent;
                    XElement path = el.Descendants("Path").FirstOrDefault();
                    XElement pathNode = path.Descendants("PathNode").FirstOrDefault();
                    XElement snippet = pathNode.Descendants("Snippet").FirstOrDefault();
                    XElement line = snippet.Descendants("Line").FirstOrDefault();

                    long SimilarityId = Convert.ToInt64(path.Attribute("SimilarityId").Value.ToString());
                    string key = "Recurring-" + SimilarityId.ToString();
                    ReportResultExtended resultExtended = response.GetValueOrDefault(key);
                    if (resultExtended == null)
                    {

                        ReportResultExtended isrecurring = new ReportResultExtended()
                        {
                            Query = query.Attribute("name").Value.ToString(),
                            Group = query.Attribute("group").Value.ToString(),
                            projectName = root.Attribute("ProjectName").Value.ToString(),
                            presetName = root.Attribute("Preset").Value.ToString(),
                            teamName = root.Attribute("Team").Value.ToString(),
                            scanDate = Convert.ToDateTime(root.Attribute("ScanStart").Value.ToString()),
                            projectId = Convert.ToInt64(root.Attribute("ProjectId").Value.ToString()),
                            scanId = Convert.ToInt64(root.Attribute("ScanId").Value.ToString()),
                            resultId = Convert.ToInt64(path.Attribute("ResultId").Value.ToString()),
                            reportId = report_id,
                            nodeId = Convert.ToInt64(el.Attribute("NodeId").Value.ToString()),
                            status = el.Attribute("Status").Value.ToString(),
                            Severity = el.Attribute("Severity").Value.ToString(),
                            similarityId = Convert.ToInt64(path.Attribute("SimilarityId").Value.ToString()),
                            pathId = Convert.ToInt64(path.Attribute("PathId").Value.ToString()),
                            state = Convert.ToInt32(el.Attribute("state").Value.ToString()),
                            fileName = el.Attribute("FileName").Value.ToString(),
                            lineNo = Convert.ToInt32(el.Attribute("Line").Value.ToString()),
                            column = Convert.ToInt32(el.Attribute("Column").Value.ToString()),
                            firstLine = line.Descendants("Code").FirstOrDefault().Value.ToString(),
                            queryId = Convert.ToInt64(query.Attribute("id").Value.ToString())

                        };
                        response.Add(key, isrecurring);
                        report_output.Add(isrecurring);
                    }
                    else
                    {
                        int currentstate = Convert.ToInt32(el.Attribute("state").Value.ToString());
                        ReportResultExtended reportResult = response[key];
                        if (currentstate != reportResult.state)
                        {
                            ReportResultExtended isrecurring = new ReportResultExtended()
                            {
                                Query = query.Attribute("name").Value.ToString(),
                                Group = query.Attribute("group").Value.ToString(),
                                projectName = root.Attribute("ProjectName").Value.ToString(),
                                presetName = root.Attribute("Preset").Value.ToString(),
                                teamName = root.Attribute("Team").Value.ToString(),
                                scanDate = Convert.ToDateTime(root.Attribute("ScanStart").Value.ToString()),
                                projectId = Convert.ToInt64(root.Attribute("ProjectId").Value.ToString()),
                                scanId = Convert.ToInt64(root.Attribute("ScanId").Value.ToString()),
                                status = el.Attribute("Status").Value.ToString(),
                                nodeId = Convert.ToInt64(el.Attribute("NodeId").Value.ToString()),
                                Severity = el.Attribute("Severity").Value.ToString(),
                                resultId = Convert.ToInt64(path.Attribute("ResultId").Value.ToString()),
                                reportId = report_id,
                                similarityId = Convert.ToInt64(path.Attribute("SimilarityId").Value.ToString()),
                                pathId = Convert.ToInt64(path.Attribute("PathId").Value.ToString()),
                                state = Convert.ToInt32(el.Attribute("state").Value.ToString()),
                                fileName = el.Attribute("FileName").Value.ToString(),
                                lineNo = Convert.ToInt32(el.Attribute("Line").Value.ToString()),
                                column = Convert.ToInt32(el.Attribute("Column").Value.ToString()),
                                firstLine = line.Descendants("Code").FirstOrDefault().Value.ToString(),
                                queryId = Convert.ToInt64(query.Attribute("id").Value.ToString())

                            };
                            response[key] = isrecurring;
                            report_output.Add(isrecurring);
                        }
                    }
                }
                IEnumerable<XElement> fixedVulerability = from el in result.Descendants("Query").Descendants("Result")
                                                          select el;
                foreach (XElement el in fixedVulerability)
                {
                    XElement query = el.Parent;
                    XElement root = query.Parent;
                    XElement path = el.Descendants("Path").FirstOrDefault();
                    XElement pathNode = path.Descendants("PathNode").FirstOrDefault();
                    XElement snippet = pathNode.Descendants("Snippet").FirstOrDefault();
                    XElement line = snippet.Descendants("Line").FirstOrDefault();
                    long SimilarityId = Convert.ToInt64(path.Attribute("SimilarityId").Value.ToString());
                    ReportResultExtended isfixed = new ReportResultExtended()
                    {
                        Query = query.Attribute("name").Value.ToString(),
                        Group = query.Attribute("group").Value.ToString(),
                        projectName = root.Attribute("ProjectName").Value.ToString(),
                        presetName = root.Attribute("Preset").Value.ToString(),
                        teamName = root.Attribute("Team").Value.ToString(),
                        scanDate = Convert.ToDateTime(root.Attribute("ScanStart").Value.ToString()),
                        projectId = Convert.ToInt64(root.Attribute("ProjectId").Value.ToString()),
                        scanId = Convert.ToInt64(root.Attribute("ScanId").Value.ToString()),
                        status = el.Attribute("Status").Value.ToString(),
                        Severity = el.Attribute("Severity").Value.ToString(),
                        resultId = Convert.ToInt64(path.Attribute("ResultId").Value.ToString()),
                        reportId = report_id,
                        nodeId = Convert.ToInt64(el.Attribute("NodeId").Value.ToString()),
                        similarityId = Convert.ToInt64(path.Attribute("SimilarityId").Value.ToString()),
                        pathId = Convert.ToInt64(path.Attribute("PathId").Value.ToString()),
                        state = Convert.ToInt32(el.Attribute("state").Value.ToString()),
                        fileName = el.Attribute("FileName").Value.ToString(),
                        lineNo = Convert.ToInt32(el.Attribute("Line").Value.ToString()),
                        column = Convert.ToInt32(el.Attribute("Column").Value.ToString()),
                        firstLine = line.Descendants("Code").FirstOrDefault().Value.ToString(),
                        queryId = Convert.ToInt64(query.Attribute("id").Value.ToString())
                    };
                    string mix = String.Format("{0}-{1}-{2}-{3}-{4}",isfixed.projectId,isfixed.queryId,isfixed.lineNo,isfixed.column,isfixed.similarityId);
                    if (!fix.ContainsKey(isfixed.projectId))
                    {
                        fix.Add(isfixed.projectId, new Dictionary<DateTime, Dictionary<string, ReportResultExtended>>());
                        fix[isfixed.projectId].Add(isfixed.scanDate, new Dictionary<string, ReportResultExtended>());
                        fix[isfixed.projectId][isfixed.scanDate].Add(mix, isfixed);
                        if (token.debug && token.verbosity > 0) { Console.WriteLine("Unique keys: {0}, {1}, {2} {3} {4} {5}", isfixed.projectName, isfixed.similarityId, isfixed.projectId, isfixed.scanId, isfixed.queryId, isfixed.scanDate); }
                    }
                    else
                    {
                        if (!fix[isfixed.projectId].ContainsKey(isfixed.scanDate))
                        {
                            fix[isfixed.projectId].Add(isfixed.scanDate, new Dictionary<string, ReportResultExtended>());
                        }
                        if (!fix[isfixed.projectId][isfixed.scanDate].TryAdd(mix, isfixed))
                        {
                            if (token.debug && token.verbosity > 0) { Console.WriteLine("Duplicate keys: {0}, {1}, {2} {3} {4} {5}", isfixed.projectName, isfixed.similarityId, isfixed.nodeId, isfixed.scanId, isfixed.queryId, isfixed.scanDate); }
                        }
                    }
                }

                return true;
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine(ex.Message);
                return false;
            }

        }
        private bool addFixed(Dictionary<long, Dictionary<DateTime, Dictionary<string, ReportResultExtended>>> fix, List<ReportResultExtended> report_output)
        {
            foreach (KeyValuePair<long, Dictionary<DateTime, Dictionary<string, ReportResultExtended>>> projects in fix)
            {
                Dictionary<DateTime, Dictionary<string, ReportResultExtended>> scanDate = projects.Value;
                var scan_date = from entry in scanDate orderby entry.Key ascending select entry;
                KeyValuePair<DateTime, Dictionary<string, ReportResultExtended>> keyValuePair = new KeyValuePair<DateTime, Dictionary<string, ReportResultExtended>>();

                foreach (KeyValuePair<DateTime, Dictionary<string, ReportResultExtended>> kv_dt in scan_date)
                {
                    if (keyValuePair.Key != DateTime.MinValue)
                    {
                        Dictionary<string, ReportResultExtended> last_scan = keyValuePair.Value;
                        Dictionary<string, ReportResultExtended> current_scan = kv_dt.Value;
                        if (token.debug && token.verbosity > 0) { Console.WriteLine("Compare: {0} {1}", keyValuePair.Key, kv_dt.Key); }
                        foreach (string key in last_scan.Keys)
                        {
                            if (token.debug && token.verbosity > 0) { Console.WriteLine("Project {0}, key {1}", last_scan[key].projectName, key); }
                            if (!current_scan.ContainsKey(key))
                            {
                                ReportResultExtended reportResult = last_scan[key];
                                reportResult.status = "Fixed";
                                report_output.Add(reportResult);
                            }
                        }
                    }
                    keyValuePair = kv_dt;
                }
            }
            return true;
        }
        public void Dispose()
        {

        }

    }
}