using CxRestClient;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;

namespace CxAnalytics.TransformLogic
{
    /// <summary>
    /// A class that implements the data transformation.
    /// </summary>
    public class Transformer
    {
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
            // TODO: Scan collection logic needs to use the CancellationToken.

            var scansToProcess = GetListOfScans(previousStatePath, ctx);

            var project_info_out = outFactory.newInstance(records.ProjectInfo);
            OutputProjectInfoRecords(scansToProcess, project_info_out);

            var scan_summary_out = outFactory.newInstance(records.SASTScanSummary);

            foreach (var scanRecord in scansToProcess.ScanDesciptors)
            {
                Dictionary<String, String> flat = new Dictionary<string, string>();
                AddPrimaryKeyElements(scanRecord, flat);
                flat.Add("ScanId", scanRecord.ScanId);
                flat.Add("ScanProduct", scanRecord.ScanProduct);
                flat.Add("ScanType", scanRecord.ScanType);
                flat.Add("FinishTime", scanRecord.FinishedStamp.ToString(DATE_FORMAT));
                flat.Add("StartTime", scansToProcess.ScanDataDetails[scanRecord.ScanId].StartTime.ToString(DATE_FORMAT));
                flat.Add("ScanRisk", scansToProcess.ScanDataDetails[scanRecord.ScanId].ScanRisk.ToString());
                flat.Add("ScanRiskSeverity", scansToProcess.ScanDataDetails[scanRecord.ScanId].ScanRiskSeverity.ToString());
                flat.Add("LinesOfCode", scansToProcess.ScanDataDetails[scanRecord.ScanId].LinesOfCode.ToString());
                flat.Add("FailedLinesOfCode", scansToProcess.ScanDataDetails[scanRecord.ScanId].FailedLinesOfCode.ToString());
                flat.Add("FileCount", scansToProcess.ScanDataDetails[scanRecord.ScanId].FileCount.ToString());
                flat.Add("CxVersion", scansToProcess.ScanDataDetails[scanRecord.ScanId].CxVersion);
                flat.Add("Languages", scansToProcess.ScanDataDetails[scanRecord.ScanId].Languages);


                // TODO: High, Med, Low, Preset (specific to scan, not project)

                scan_summary_out.write(flat);
            }

        }

        private static void OutputProjectInfoRecords(ScanData scansToProcess, IOutput project_info_out)
        {
            Dictionary<int, bool> processed = new Dictionary<int, bool>();
            foreach (var scanRecord in scansToProcess.ScanDesciptors)
            {
                if (!processed.TryAdd(scanRecord.Project.ProjectId, true))
                    continue;

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
        }

        private static void AddPrimaryKeyElements(ScanDescriptor rec, Dictionary<string, string> flat)
        {
            flat.Add("ProjectId", rec.Project.ProjectId.ToString());
            flat.Add("ProjectName", rec.Project.ProjectName);
            flat.Add("TeamName", rec.Project.TeamName);
        }

        private class ScanData
        {
            public ScanData ()
            {
                ScanDataDetails = new Dictionary<string, CxSastScans.Scan>();
            }

            public IEnumerable<ScanDescriptor> ScanDesciptors { get; set; }
            public Dictionary<String, CxSastScans.Scan> ScanDataDetails { get; private set; }
        }

        private static ScanData GetListOfScans(string previousStatePath, CxRestContext ctx)
        {
            DateTime checkStart = DateTime.Now;

            // Populate the data resolver with teams and presets
            DataResolver dr = new DataResolver();

            var presetEnum = CxPresets.GetPresets(ctx);

            foreach (var preset in presetEnum)
                dr.addPreset(preset.PresetId, preset.PresetName);

            var teamEnum = CxTeams.GetTeams(ctx);

            foreach (var team in teamEnum)
                dr.addTeam(team.TeamId, team.TeamName);

            // Now populate the project resolver with the projects
            ProjectResolver pr = dr.Resolve(previousStatePath);

            var projects = CxProjects.GetProjects(ctx);

            foreach (var p in projects)
                pr.addProject(p.TeamId, p.PresetId, p.ProjectId, p.ProjectName);

            // Resolve projects to get the scan resolver.
            ScanResolver sr = pr.Resolve();

            ScanData retVal = new ScanData();

            // Get SAST and SCA scans
            var sastScans = CxSastScans.GetScans(ctx, CxSastScans.ScanStatus.Finished);

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
