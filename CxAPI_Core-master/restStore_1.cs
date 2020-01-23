using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Text;
using System.Xml.Linq;
using CxAPI_Core.dto;
using System.Threading;
using System.Runtime.Serialization.Formatters.Binary;

namespace CxAPI_Core
{
    class restStore_1 : IDisposable
    {
        public resultClass token;

        public restStore_1(resultClass token)
        {
            this.token = token;
        }

        public bool fetchResultsbyDate()
        {
            bool waitFlag = false;
            List<ReportTrace> trace = new List<ReportTrace>();
            getScanResults scanResults = new getScanResults();
            getScans scans = new getScans();
            List<ScanObject> scan = scans.getScan(token);
            ConsoleSpinner spinner = new ConsoleSpinner();
        

            foreach (ScanObject s in scan)
            {
                if ((s.DateAndTime != null) && (s.Status.Id == 7) && (s.DateAndTime.StartedOn > token.start_time) && (s.DateAndTime.StartedOn < token.end_time))
                {
                    if ((String.IsNullOrEmpty(token.project_name) || ((!String.IsNullOrEmpty(token.project_name)) && (s.Project.Name.Contains(token.project_name)))))
                    {
                        if (token.save_result.Contains("XML"))
                        {
                            ReportResult result = scanResults.SetResultRequest(s.Id, "XML", token);
                            if (result != null)
                            {
                                trace.Add(new ReportTrace(s.Project.Id, s.Project.Name, s.DateAndTime.StartedOn, s.Id, result.ReportId, "XML"));
                            }
                        }
                        if (token.save_result.Contains("PDF"))
                        {
                            ReportResult result = scanResults.SetResultRequest(s.Id, "PDF", token);
                            if (result != null)
                            {
                                trace.Add(new ReportTrace(s.Project.Id, s.Project.Name, s.DateAndTime.StartedOn, s.Id, result.ReportId, "PDF"));
                            }
                        }
                    }
                }
            }

            while (!waitFlag)
            {
                spinner.Turn();
                if (token.debug && token.verbosity > 0) { Console.WriteLine("Sleeping 1 second(s)");}
                Thread.Sleep(1000);
                waitFlag = true;
                foreach (ReportTrace rt in trace)
                {
                    if (!rt.isRead)
                    {
                        waitFlag = false;
                        if (token.debug && token.verbosity > 0) { Console.WriteLine("Testing report.Id {0}", rt.reportId); }
                        if (scanResults.GetResultStatus(rt.reportId, token))
                        {
                            if (token.debug && token.verbosity > 0) { Console.WriteLine("Found report.Id {0}", rt.reportId); }
                            var result = scanResults.GetGenaricResult(rt.reportId, token);
                            if (result != null)
                            {
                                rt.isRead = true;
                                writeOutputToFile(rt, token);
                                trace.Remove(rt);
                                break;
                            }
                        }
                    }
                }
            }
            return true;
        }

        private bool writeOutputToFile(ReportTrace rt, resultClass token)
        {
            try
            {
                if ((!String.IsNullOrEmpty(token.save_result)) && (!String.IsNullOrEmpty(token.save_result_path)))
                {
                    if (rt.reportType == "XML")
                    {
                        XElement xl = XElement.Parse(token.op_result);
                        string filename = token.save_result_path + @"\" + rt.projectName + '-' + rt.scanTime.Value.ToString("yyyyMMddhhmmss") + ".xml";
                        File.WriteAllText(filename, xl.ToString(), System.Text.Encoding.UTF8);
                        return true;
                    }
                    else if (rt.reportType == "PDF")
                    { 
                        string filename = token.save_result_path + @"\" + rt.projectName + '-' + rt.scanTime.Value.ToString("yyyyMMddhhmmss") + ".pdf";
                        File.WriteAllBytes(filename, token.byte_result);
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine(ex.Message);
                throw ex;
            }
            return false;
        }

        private bool writePDFOutput(ReportTrace rt, XElement result)
        {
            try
            {
                if ((!String.IsNullOrEmpty(token.save_result)) && (!String.IsNullOrEmpty(token.save_result_path)))
                {
                    string filename = token.save_result_path + @"\" + rt.projectName + '-' + rt.scanTime.Value.ToString("yyyyMMddhhmmss") + ".pdf";
                    File.WriteAllText(filename, result.ToString(), System.Text.Encoding.UTF8);
                    return true;
                }
                else
                {
                    return false;
                }
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine(ex.Message);
                throw ex;
            }
        }

        public void Dispose()
        {

        }

    }

}