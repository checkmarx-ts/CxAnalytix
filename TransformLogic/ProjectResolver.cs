using log4net;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace CxAnalytics.TransformLogic
{
    /// <summary>
    /// A class that is used to correlate projects with the team and preset details.
    /// </summary>
    /// <remarks>
    /// This class handles the persistence of project checks between executions of data resolution.
    /// </remarks>
    public class ProjectResolver
    {
        private static readonly String STORAGE_FILE = "CxAnalytixExportState.json";

        private static ILog _log = LogManager.GetLogger(typeof(ProjectResolver));

        private DataResolver _dependentData;

        private bool _disallowAdd = false;

        private String _stateStorageFilePath = null;

        private StreamWriter _nextStateOutputStream = null;

        private ProjectResolver ()
        {
        }

        internal ProjectResolver(DataResolver depData, String previousStateStoragePath)
        {
            _dependentData = depData;

            if (previousStateStoragePath != null)
                _stateStorageFilePath = Path.Combine(previousStateStoragePath, STORAGE_FILE);

            if (previousStateStoragePath != null && System.IO.File.Exists(_stateStorageFilePath))
            {
                using (var sr = new StreamReader(_stateStorageFilePath))
                    loadProjectCheckState(sr);
            }
            else
                _previousTargets = new Dictionary<int, ProjectDescriptorExt>();
        }

        internal ProjectResolver(DataResolver depData, StreamReader previousStateDataStream, 
            StreamWriter nextStateDataStream)
        {
            _dependentData = depData;
            _nextStateOutputStream = nextStateDataStream;

            if (previousStateDataStream != null)
                loadProjectCheckState(previousStateDataStream);
            else
                _previousTargets = new Dictionary<int, ProjectDescriptorExt>();
        }


        private void loadProjectCheckState (StreamReader sr)
        {
            var serializer = JsonSerializer.Create();
            _previousTargets = serializer.Deserialize(sr,
                typeof(Dictionary<int, ProjectDescriptorExt>))
                as Dictionary<int, ProjectDescriptorExt>;
        }

        internal void saveProjectCheckState()
        {
            if (_stateStorageFilePath == null && _nextStateOutputStream == null)
            {
                _log.Warn("No path to the state file was given, state is not preserved.");
                return;
            }

            StreamWriter outStream = null;

            bool closeStream = false;

            if (_stateStorageFilePath != null)
            {
                outStream = new StreamWriter(_stateStorageFilePath);
                closeStream = true;
            }
            else
                outStream = _nextStateOutputStream;

            var serializer = JsonSerializer.Create();
            serializer.Serialize(outStream, _targets);
            outStream.Flush();

            if (closeStream)
                outStream.Close();
        }


        private ScanResolver _res = null;

        /// <summary>
        /// Executes logic to determine which projects are candidates for querying for scan results
        /// since the last run.
        /// </summary>
        /// <remarks>
        /// Each run will have a list of projects currently in the system.  There may be new projects
        /// since the last run that have never been checked for scans.  Some projects may have been
        /// deleted since the last run and need not be checked.
        /// </remarks>
        /// <returns>An instance of <see cref="ScanResolver"/> that will assist with
        /// picking scans that need to be used in the transformation.</returns>
        public ScanResolver Resolve (Dictionary<String, Action<ScanDescriptor, Transformer>> productAction)
        {
            if (_res == null && !_disallowAdd)
            {
                _disallowAdd = true;
                _targets = new Dictionary<int, ProjectDescriptorExt>();
                int newProjects = 0;

                foreach (int pid in _newProjects.Keys)
                {
                    // Look for the project in the previous projects.  If it exists, use that record to make
                    // the project a target for scan pulling.
                    if (_previousTargets.ContainsKey(pid))
                    {
                        _targets.Add(pid, _previousTargets[pid]);
                        _previousTargets.Remove(pid);
                    }
                    else
                    {
                        // If the project was never seen before, it is a new project and a target for pulling scans.
                        _targets.Add(pid, ProjectDescriptorExt.newDetail(_newProjects[pid]));
                        newProjects++;
                    }
                }

                // scanTargets now contains the projects that were added via addProject.  If they were checked before,
                // the last check date is in the record.  If not, the last check date is the epoch.

                // _previousProjects now has projects that have likely been deleted, so throw some INFO in the log
                // that these aren't going to be scanned.
                _log.InfoFormat("{0} projects are targets for check for new scans. Since last scan: {1} projects removed, {2} new projects.",
                    _targets.Keys.Count, _previousTargets.Keys.Count, newProjects);

                foreach (int pid in _previousTargets.Keys)
                {
                    _log.InfoFormat("No longer tracking state for project {0}:[{1}] Team [{1}]",
                        pid, _previousTargets[pid].ProjectName, _previousTargets[pid].TeamName);
                }

                _res = new ScanResolver(this, productAction);
            }

            return _res;
        }

        private Dictionary<int, ProjectDescriptorExt> _targets;
        internal Dictionary<int, ProjectDescriptorExt> Targets
        {
            get => _targets;
        }

        private Dictionary<int, ProjectDescriptorExt> _previousTargets;

        private Dictionary<int, ProjectDescriptor> _newProjects = new Dictionary<int, ProjectDescriptor>();

        /// <summary>
        /// Adds a project that is currently in the system.
        /// </summary>
        /// <param name="teamId">The GUID team identifier for the project.</param>
        /// <param name="presetId">The numeric preset identifier set for the project.</param>
        /// <param name="projectId">The numeric project identifier.</param>
        /// <param name="projectName">The name of the project.</param>
        /// <returns>True of the project was added, false otherwise.</returns>
        public bool AddProject(Guid teamId, int presetId, int projectId, String projectName, 
            String policies)
        {
            if (projectName == null)
                return false;

            if (!_disallowAdd)
                if (_newProjects.ContainsKey(projectId))
                {
                    _log.WarnFormat("Rejected changed when adding new project with duplicate id {0}: New name: [{1}] current name: [{2}].",
                        projectId, projectName, _newProjects[projectId].ProjectName);
                    return false;
                }
                else
                {
                    String teamName = _dependentData.Teams.ContainsKey(teamId) ? _dependentData.Teams[teamId] : String.Empty;
                    if (String.Empty == teamName)
                    {
                        _log.ErrorFormat("Unable to find a team name for team id [{0}] when adding project {1}:{2}", teamId,
                            projectId, projectName);

                        return false;
                    }

                    String presetName = _dependentData.Presets.ContainsKey(presetId) ? _dependentData.Presets[presetId] : String.Empty;
                    if (String.Empty == presetName)
                    {
                        _log.ErrorFormat("Unable to find a preset name for preset id [{0}] when adding project {1}:{2}", presetId,
                            projectId, projectName);

                        return false;
                    }

                    _newProjects.Add(projectId,
                        new ProjectDescriptor()
                        {
                            ProjectId = projectId,
                            ProjectName = projectName,
                            TeamName = teamName,
                            TeamId = teamId,
                            PresetId = presetId,
                            PresetName = presetName,
                            Policies = policies
                        }
                    );

                }

            return !_disallowAdd;
        }

    }
}
