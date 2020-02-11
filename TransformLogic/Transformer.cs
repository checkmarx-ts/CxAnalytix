using CxRestClient;
using log4net;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;

namespace CxAnalytics.TransformLogic
{
    /// <summary>
    /// A class that implements the data transformation.
    /// </summary>
    public class Transformer
    {
        private static ILog _log = LogManager.GetLogger(typeof (Transformer) );

        private static readonly String DATE_FORMAT = "yyyy-MM-ddTHH:mm:ss.fffzzz";
        /// <summary>
        /// The main logic for invoking a transformation.  It does not return until a sweep
        /// for new scans is performed across all projects.
        /// </summary>
        /// <param name="concurrentThreads">The number of concurrent scan transformation threads.</param>
        /// <param name="previousStatePath">A folder path where files will be created to store any state
        /// data required to resume operations across program runs.</param>
        /// <param name="ctx"></param>
        /// <param name="outFactory">The factory implementation for making IOutput instances
        /// used for outputting various record types.</param>
        /// <param name="records">The names of the supported record types that will be used by 
        /// the IOutputFactory to create the correct output implementation instance.</param>
        /// <param name="token">A cancellation token that can be used to stop processing of data if
        /// the task needs to be interrupted.</param>
        public static void doTransform(int concurrentThreads, String previousStatePath,
            CxRestContext ctx, IOutputFactory outFactory, RecordNames records, CancellationToken token)
        {
            // TODO: Some reports may not be generated for some reason, so we skip compiling 
            // details of that scan in instances where the report is not created.
            
            ParallelOptions threadOpts = new ParallelOptions()
            {
                CancellationToken = token,
                MaxDegreeOfParallelism = concurrentThreads
            };

            var scansToProcess = GetListOfScans(previousStatePath, ctx, token);

            ConcurrentDictionary<int, bool> processedProjects = 
                new ConcurrentDictionary<int, bool>();

            var project_info_out = outFactory.newInstance(records.ProjectInfo);
            var scan_summary_out = outFactory.newInstance(records.SASTScanSummary);

            Parallel.ForEach<ScanDescriptor>(scansToProcess.ScanDesciptors, threadOpts,
                (scan) =>
                {
                    var report = CxSastXmlReport.GetXmlReport(ctx, token, scan.ScanId);

                    ProcessReport(scan, report);

                    if (processedProjects.TryAdd(scan.Project.ProjectId, true))
                        OutputProjectInfoRecords(scan, project_info_out);

                    OutputScanSummary(scan, scansToProcess, scan_summary_out);
                }

                );


        }


        private static void ProcessReport(ScanDescriptor scan, Stream report)
        {

            using (XmlReader xr = XmlReader.Create(report))
                while (xr.Read())
                {
                    if (xr.NodeType == XmlNodeType.Element)
                    {
                        if (xr.Name.CompareTo("CxXMLResults") == 0)
                        {
                            _log.Debug("Processing attributes in CxXMLResults.");
                            scan.Preset = xr.GetAttribute("Preset");
                        }

                        if (xr.Name.CompareTo("Result") == 0)
                            scan.IncrementSeverity(xr.GetAttribute("Severity"));

                    }

                }
        }

        private static void OutputScanSummary(ScanDescriptor scanRecord, ScanData scansToProcess, 
            IOutput scan_summary_out)
        {
            Dictionary<String, String> flat = new Dictionary<string, string>();
            AddPrimaryKeyElements(scanRecord, flat);
            flat.Add("ScanId", scanRecord.ScanId);
            flat.Add("ScanProduct", scanRecord.ScanProduct);
            flat.Add("ScanType", scanRecord.ScanType);
            flat.Add("ScanFinished", scanRecord.FinishedStamp.ToString(DATE_FORMAT));
            flat.Add("ScanStarted", scansToProcess.ScanDataDetails[scanRecord.ScanId].StartTime.ToString(DATE_FORMAT));
            flat.Add("ScanRisk", scansToProcess.ScanDataDetails[scanRecord.ScanId].ScanRisk.ToString());
            flat.Add("ScanRiskSeverity", scansToProcess.ScanDataDetails[scanRecord.ScanId].ScanRiskSeverity.ToString());
            flat.Add("LinesOfCode", scansToProcess.ScanDataDetails[scanRecord.ScanId].LinesOfCode.ToString());
            flat.Add("FailedLinesOfCode", scansToProcess.ScanDataDetails[scanRecord.ScanId].FailedLinesOfCode.ToString());
            flat.Add("FileCount", scansToProcess.ScanDataDetails[scanRecord.ScanId].FileCount.ToString());
            flat.Add("CxVersion", scansToProcess.ScanDataDetails[scanRecord.ScanId].CxVersion);
            flat.Add("Languages", scansToProcess.ScanDataDetails[scanRecord.ScanId].Languages);
            flat.Add("Preset", scanRecord.Preset);
            foreach (var sev in scanRecord.SeverityCounts.Keys)
                flat.Add(sev, Convert.ToString(scanRecord.SeverityCounts[sev]));

            scan_summary_out.write(flat);
        }

        private static void OutputProjectInfoRecords(ScanDescriptor scanRecord, IOutput project_info_out)
        {
            Dictionary<String, String> flat = new Dictionary<string, string>();
            AddPrimaryKeyElements(scanRecord, flat);

            flat.Add("PresetName", scanRecord.Project.PresetName);

            foreach (var lastScanProduct in scanRecord.Project.LatestScanDateByProduct.Keys)
                flat.Add($"{lastScanProduct}_LastScanDate",
                    scanRecord.Project.LatestScanDateByProduct[lastScanProduct].ToString(DATE_FORMAT));

            foreach (var scanCountProduct in scanRecord.Project.ScanCountByProduct.Keys)
                flat.Add($"{scanCountProduct}_Scans",
                    scanRecord.Project.ScanCountByProduct[scanCountProduct].ToString());

            project_info_out.write(flat);

        }

        private static void AddPrimaryKeyElements(ScanDescriptor rec, Dictionary<string, string> flat)
        {
            flat.Add("ProjectId", rec.Project.ProjectId.ToString());
            flat.Add("ProjectName", rec.Project.ProjectName);
            flat.Add("TeamName", rec.Project.TeamName);
        }

        private class ScanData
        {
            public ScanData()
            {
                ScanDataDetails = new Dictionary<string, CxSastScans.Scan>();
            }

            public IEnumerable<ScanDescriptor> ScanDesciptors { get; set; }
            public Dictionary<String, CxSastScans.Scan> ScanDataDetails { get; private set; }
        }

        private static ScanData GetListOfScans(string previousStatePath, CxRestContext ctx,
            CancellationToken token)
        {
            DateTime checkStart = DateTime.Now;

            // Populate the data resolver with teams and presets
            DataResolver dr = new DataResolver();

            var presetEnum = CxPresets.GetPresets(ctx, token);

            foreach (var preset in presetEnum)
                dr.addPreset(preset.PresetId, preset.PresetName);

            var teamEnum = CxTeams.GetTeams(ctx, token);

            foreach (var team in teamEnum)
                dr.addTeam(team.TeamId, team.TeamName);

            // Now populate the project resolver with the projects
            ProjectResolver pr = dr.Resolve(previousStatePath);

            var projects = CxProjects.GetProjects(ctx, token);

            foreach (var p in projects)
                pr.addProject(p.TeamId, p.PresetId, p.ProjectId, p.ProjectName);

            // Resolve projects to get the scan resolver.
            ScanResolver sr = pr.Resolve();

            ScanData retVal = new ScanData();

            // Get SAST and SCA scans
            var sastScans = CxSastScans.GetScans(ctx, token, CxSastScans.ScanStatus.Finished);

            foreach (var sastScan in sastScans)
            {
                sr.addScan(sastScan.ProjectId, sastScan.ScanType, "SAST", sastScan.ScanId, sastScan.FinishTime);
                retVal.ScanDataDetails.Add(sastScan.ScanId, sastScan);
            }

            // TODO: SCA scans need to be loaded and added to the ScanResolver instance.


            // Get the scans to process, update the last check date in all projects.
            retVal.ScanDesciptors = sr.Resolve(checkStart);

            return retVal;
        }
    }
}
