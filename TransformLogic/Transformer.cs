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
        // TODO: Pass in the configuration parameters collected from the caller (via app.config,
        // command line options, or other methods of collecting config data)
        /// <summary>
        /// The main logic for invoking a transformation.  It does not return until a sweep
        /// for new scans is performed across all projects.
        /// </summary>
        /// <param name="concurrentThreads">The number of concurrent scan transformation threads.</param>
        /// <param name="outFactory">The factory implementation for making IOutput instances
        /// used for outputting various record types.</param>
        /// <param name="token">A cancellation token that can be used to stop processing of data if
        /// the task needs to be interrupted.</param>
        public static void doTransform (int concurrentThreads, String previousStatePath, 
            CxRestContext ctx, IOutputFactory outFactory, CancellationToken token)
        {
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

        }
    }
}
