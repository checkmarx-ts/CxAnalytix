using CxAnalytix.Exceptions;
using log4net;
using Newtonsoft.Json;
using SDK.Modules.Transformer.Data;
using System;
using System.Collections.Concurrent;
using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("TransformLogic_Tests")]
namespace SDK.Modules.Transformer
{
	public class CrawlState
	{
		private static readonly ILog _log = LogManager.GetLogger(typeof(CrawlState));


		// Holds the current state of crawls per project.
		private ConcurrentDictionary<String, ProjectDescriptorExt> _currentState = new ConcurrentDictionary<String, ProjectDescriptorExt>();
		private ConcurrentDictionary<String, ProjectDescriptorExt> _confirmedProjects = new ConcurrentDictionary<String, ProjectDescriptorExt>();
		private ConcurrentDictionary<String, ScanDescriptor> _scans = new ConcurrentDictionary<string, ScanDescriptor>();
		private ConcurrentDictionary<String, SortedSet<ScanDescriptor>> _projectScanIndex = new ConcurrentDictionary<String, SortedSet<ScanDescriptor>>();

		public int ScanCount { get => _scans.Count;  }

		public int NewProjects { get; private set; }

		public int DeletedProjects { get; private set; }

        private String _storageFile;
        private String _tempStorageFile;
		private String _recoveryStorageFile;

		private bool _confirmed = false;


		public int ProjectCount { get => _currentState.Count; }

		public DateTime OldestScanCheckDate { get; internal set; } = DateTime.MinValue;

		public ICollection<ProjectDescriptorExt> Projects { get => _currentState.Values; }


		// Confirms a project was found in the remote system.  This allows projects previously crawled to
		// be removed if they were deleted from the remote system.
		[MethodImpl(MethodImplOptions.Synchronized)]
		public void ConfirmProjects(IReadOnlyCollection<ProjectDescriptor> projects)
		{
			if (_confirmed)
				throw new InvalidOperationException("The crawl state has already been confirmed.");

			var currentStateClone = new ConcurrentDictionary<String, ProjectDescriptorExt>(_currentState);

			foreach (var project in projects)
			{
				// project was found previously, store the updated state.
				if (currentStateClone.ContainsKey(project.ProjectId))
				{
					// Find the earliest scan check, based on previous scan checks.
					if (OldestScanCheckDate == DateTime.MinValue || OldestScanCheckDate.CompareTo(currentStateClone[project.ProjectId].LastScanCheckDate) < 0)
                        OldestScanCheckDate = currentStateClone[project.ProjectId].LastScanCheckDate;


                    if (!_confirmedProjects.TryAdd(project.ProjectId, new ProjectDescriptorExt(project, currentStateClone[project.ProjectId])))
						throw new UnrecoverableOperationException($"TryAdd returned false trying to add ProjectId: {project.ProjectId}");

					ProjectDescriptorExt removed;
					if (!currentStateClone.TryRemove(project.ProjectId, out removed))
						throw new UnrecoverableOperationException($"TryRemove returned false trying to remove {project.ProjectId}");
				}
				else
				{
					// New project
					if (!_confirmedProjects.TryAdd(project.ProjectId, new ProjectDescriptorExt(project)))
						throw new UnrecoverableOperationException($"TryAdd returned false trying to add new project {project.ProjectId}");
					NewProjects++;
				}

				if (!_projectScanIndex.TryAdd(project.ProjectId, new SortedSet<ScanDescriptor>()) )
					throw new UnrecoverableOperationException($"TryAdd returned false trying to create project index for {project.ProjectId}");
			}

			DeletedProjects = currentStateClone.Count;

			_confirmed = true;
			_currentState = _confirmedProjects;
			_confirmedProjects = null;

			Persist();
		}

		public void ScanCompleted (ScanDescriptor scan)
		{
			if (!_confirmed)
				throw new InvalidOperationException("The crawl state has not yet been confirmed.");

			if (_currentState.ContainsKey(scan.Project.ProjectId))
				_currentState[scan.Project.ProjectId].LastScanCheckDate = scan.FinishedStamp;
			else
				throw new UnrecoverableOperationException($"Project Id {scan.Project.ProjectId} not found trying to mark scan {scan.ScanId} completed.");

			Persist();
		}

		public CrawlState (String storageFilePath, String storageFileName)
		{
            _storageFile = Path.Combine(storageFilePath, storageFileName);
            _tempStorageFile = $"{_storageFile}.tmp";
			_recoveryStorageFile = $"{_storageFile}.recovery";

			_log.Debug($"Crawl state file here: [{_storageFile}] Temp file: [{_tempStorageFile}] Recovery file: [{_recoveryStorageFile}]");

			bool recoveryPresent = File.Exists(_recoveryStorageFile);

            if (File.Exists (_tempStorageFile) )
			{
                _log.Warn($"Temp state file {_tempStorageFile} exists, deleting.");
                File.Delete(_tempStorageFile);
			}

			String firstFileToLoad = String.Empty;



			if (File.Exists(_storageFile))
				firstFileToLoad = _storageFile;
			else if (recoveryPresent)
			{
				_log.Warn("State file is missing but recovery is present, loading the state from the recovery file.");
				firstFileToLoad = _recoveryStorageFile;
			}
			else
				return; // No files present, this may be the first run.


			try
			{
				_currentState = loadCrawlState(firstFileToLoad);
				if (_currentState != null && _currentState.Count == 0 && recoveryPresent)
				{
					var tempState = loadCrawlState(_recoveryStorageFile);
					if (tempState != null && tempState.Count > 0)
					{
						_currentState = tempState;
						_log.Warn("State file had 0 records, current state was loaded from the recovery file.");
					}
				}
			}
			catch (Newtonsoft.Json.JsonSerializationException ex)
			{
				if (!recoveryPresent)
					throw new UnrecoverableOperationException("Unable to load state file and recovery file is not present.", ex);

				_log.Warn($"State file [{_storageFile}] appears to be corrupt, attempting to recover from [{_recoveryStorageFile}]");
				_currentState = loadCrawlState(_recoveryStorageFile);
				Persist();
				_log.Warn("State was recovered from the recovery file. ");
			}
		}


		private static ConcurrentDictionary<String, ProjectDescriptorExt> loadCrawlState(String filePath)
        {
            var serializer = JsonSerializer.Create();
            using (var sr = new StreamReader (filePath) )
				return serializer.Deserialize(sr,
					typeof(ConcurrentDictionary<String, ProjectDescriptorExt>))
					as ConcurrentDictionary<String, ProjectDescriptorExt>;
		}

		public IEnumerable<ScanDescriptor> GetScansForProject(String projectId)
			=> _projectScanIndex[projectId];

		public int GetScanCountForProject(String projectId) => _projectScanIndex[projectId].Count;


		public bool AddScan(String projectId, String scanType, ScanDescriptor.ScanProductType scanProduct, String scanId, DateTime finishTime, String engine)
		{
			if (!_confirmed)
				throw new InvalidOperationException("The crawl state has not yet been confirmed.");

			if (scanType == null || scanId == null)
				return false;

			// Scans for deleted projects are skipped.
			if (!_currentState.ContainsKey(projectId))
				return false;

			if (_scans.ContainsKey(scanId))
			{
				_log.Warn($"Attempted to add a duplicate scan id [{scanId}] for project {projectId}:[{_currentState[projectId].ProjectName}].");
				return false;
			}

			// Always update the last scan time for a product if there was a scan for it.
			_currentState[projectId].UpdateLatestScanDate(scanProduct, finishTime);

			// Filter old scans
			if (_currentState[projectId].LastScanCheckDate.CompareTo(finishTime) < 0)
			{
				_currentState[projectId].IncrementScanCount(scanProduct);

				var descriptor = new ScanDescriptor()
				{
					Project = _currentState[projectId],
					ScanType = scanType,
					ScanProduct = scanProduct,
					ScanId = scanId,
					FinishedStamp = finishTime,
					Engine = engine

				};

				if (!_scans.TryAdd(scanId, descriptor))
					_log.Warn($"Unexpected false result attempting to add Scan ID {scanId} for {scanProduct} to the scans to crawl.");

				if (!_projectScanIndex.ContainsKey(projectId))
					_log.Warn($"Scan index for project {projectId} does not exist, scans will not be crawled.");
				_projectScanIndex[projectId].Add(descriptor);
			}

			return true;
		}

		[MethodImpl(MethodImplOptions.Synchronized)]
		private void Persist ()
		{
			// Persisting the state storage file is not transactional.  This attempts to avoid
			// cases where the wrong state is recorded if the process dies and causes
			// the state storage file to become corrupted.

			// Write the state out to the temporary file first.  If this is interrupted, the state
			// won't change.
			using (var outStream = new StreamWriter(_tempStorageFile) )
			{
				var serializer = JsonSerializer.Create();
				serializer.Serialize(outStream, _currentState);
				outStream.Flush();
			}

			// The temp file is now the correct current state.  Rename the previous state file to recovery
			// so if this part fails the state can be recovered.
			if (File.Exists(_storageFile))
				File.Move(_storageFile, _recoveryStorageFile, true);

			// Rename the temp file to the current state file.
			File.Move(_tempStorageFile, _storageFile, true);

			// Remove the recovery file
			File.Delete(_recoveryStorageFile);
		}

	}
}
