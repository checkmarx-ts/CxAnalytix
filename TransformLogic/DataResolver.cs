using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace CxAnalytics.TransformLogic
{
    /// <summary>
    /// The entrypoint class for resolving which projects/scans to export
    /// on the current run.
    /// </summary>
    public class DataResolver
    {
        private bool _disallowAdd = false;

        private Dictionary<int, String> _presets = new Dictionary<int, string>();

        internal Dictionary<int, String> Presets { get => _presets; }


        /// <summary>
        /// The method use to load the list of presets into memory for resolution
        /// when the Resolve method is called.
        /// </summary>
        /// <param name="presetId">The preset numeric identifier.</param>
        /// <param name="presetName">The preset name.</param>
        /// <returns>True if the preset was added, false otherwise.</returns>
        /// <see cref="DataResolver.Resolve(StreamReader, StreamWriter)"/>
        /// <see cref="DataResolver.Resolve(string)"/>
        public bool addPreset(int presetId, String presetName)
        {
            if (presetName == null)
                return false;

            if (!_disallowAdd)
                if (!_presets.ContainsKey(presetId))
                    _presets.Add(presetId, presetName);
                else
                    _presets[presetId] = presetName;

            return !_disallowAdd;
        }

        internal Dictionary<Guid, String> Teams { get => _teams; }

        private Dictionary<Guid, String> _teams = new Dictionary<Guid, string>();
        /// <summary>
        /// The method use to load the list of teams into memory for resolution
        /// when the Resolve method is called.
        /// </summary>
        /// <param name="teamId">The GUID team identifier.</param>
        /// <param name="teamName">The name of the team.</param>
        /// <returns>True if the team was added, false otherwise.</returns>
        /// <see cref="DataResolver.Resolve(StreamReader, StreamWriter)"/>
        /// <see cref="DataResolver.Resolve(string)"/>
        public bool addTeam (Guid teamId, String teamName)
        {
            if (teamId == Guid.Empty || teamName == null)
                return false;

            if (!_disallowAdd)
                if (!_teams.ContainsKey(teamId))
                    _teams.Add(teamId, teamName);
                else
                    _teams[teamId] = teamName;

            return !_disallowAdd;
        }


        private ProjectResolver _res = null;

        /// <summary>
        /// The method called to create the <see cref="ProjectResolver"/> instance
        /// that will correlate project context to preset and team.
        /// </summary>
        /// <param name="previousStateFilePath">
        /// The path to the file that maintains the state between resolutions.
        /// </param>
        /// <returns>An instance of <see cref="ProjectResolver"/></returns>
        public ProjectResolver Resolve (String previousStateFilePath)
        {
            if (_res == null && !_disallowAdd)
            {
                _disallowAdd = true;
                _res = new ProjectResolver(this, previousStateFilePath);
            }

            return _res;
        }


        /// <summary>
        /// The method called to create the <see cref="ProjectResolver"/> instance
        /// that will correlate project context to preset and team.
        /// </summary>
        /// <param name="previousState">A stream of data containing the previous state.</param>
        /// <param name="nextState">A stream where data for the next state will be written.</param>
        /// <returns>An instance of <see cref="ProjectResolver"/></returns>
        public ProjectResolver Resolve(StreamReader previousState, StreamWriter nextState)
        {
            if (_res == null && !_disallowAdd)
            {
                _disallowAdd = true;
                _res = new ProjectResolver(this, previousState, nextState);
            }

            return _res;
        }

    }
}
