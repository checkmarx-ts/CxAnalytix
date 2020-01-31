using CxRestClient;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace CxAnalytics.TransformLogic
{
    /// <summary>
    /// A class that implements the data transformation.
    /// </summary>
    public class Transformer
    {
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
        /// <param name="token">A cancellation token that can be used to stop processing of data if
        /// the task needs to be interrupted.</param>
        public static void doTransform(int concurrentThreads, String previousStatePath,
            CxRestContext ctx, IOutputFactory outFactory, CancellationToken token)
        {
            // TODO: Scan collection logic needs to use the CancellationToken.

            var scansToProcess = GetListOfScans(previousStatePath, ctx);



        }

        private static IEnumerable<ScanDescriptor> GetListOfScans(string previousStatePath, CxRestContext ctx)
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

            // Get SAST and SCA scans
            var sastScans = CxSastScans.GetScans(ctx, CxSastScans.ScanStatus.Finished);

            foreach (var sastScan in sastScans)
                sr.addScan(sastScan.ProjectId, sastScan.ScanType, "SAST", sastScan.ScanId, sastScan.FinishTime);

            // TODO: SCA scans need to be loaded and added to the ScanResolver instance.


            // Get the scans to process, update the last check date in all projects.
            return sr.Resolve(checkStart);
        }
    }
}
