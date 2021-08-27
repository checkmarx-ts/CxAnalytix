using CxRestClient;
using CxRestClient.MNO;
using CxRestClient.SAST;
using CxRestClient.OSA;
using log4net;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;
using System.Text;
using CxAnalytix.TransformLogic.Data;
using CxAnalytix.Interfaces.Outputs;
using CxRestClient.MNO.dto;
using CxAnalytix.TransformLogic.Persistence;
using static CxAnalytix.TransformLogic.Data.ScanDescriptor;
using CxAnalytix.Interfaces.Transform;
using OutputBootstrapper;
using CxAnalytix.Extensions;

namespace CxAnalytix.TransformLogic
{
	/// <summary>
	/// A class that implements the data transformation.
	/// </summary>
	public class Transformer
	{
		private static readonly ILog _log = LogManager.GetLogger(typeof(Transformer));

		private IEnumerable<ScanDescriptor> ScanDescriptors { get; set; }

		private ConcurrentDictionary<String, CxSastScans.Scan> SastScanCache { get; set; }
			= new ConcurrentDictionary<string, CxSastScans.Scan>();
		private ConcurrentDictionary<String, CxOsaScans.Scan> ScaScanCache { get; set; }
			= new ConcurrentDictionary<string, CxOsaScans.Scan>();

		private ProjectPolicyIndex Policies { get; set; }
		private ConcurrentDictionary<int, String> Presets { get; set; }
		private ConcurrentDictionary<String, String> Teams { get; set; }

		private ConcurrentDictionary<int, ProjectDescriptor> _loadedProjects = new ConcurrentDictionary<int, ProjectDescriptor>();

		private CrawlState _state;


		private DateTime CheckTime { get; set; } = DateTime.Now;

		private CxRestContext RestContext { get; set; }
		private CancellationToken CancelToken { get; set; }

		ParallelOptions ThreadOpts { get; set; }

		public String InstanceId { get; set; }

		public IRecordRef ProjectInfoOut { get; internal set; }
		public IRecordRef SastScanSummaryOut { get; internal set; }
		public IRecordRef SastScanDetailOut { get; internal set; }
		public IRecordRef ScaScanSummaryOut { get; internal set; }
		public IRecordRef ScaScanDetailOut { get; internal set; }
		public IRecordRef PolicyViolationDetailOut { get; internal set; }
		public IProjectFilter Filter { get; private set; }

		private ConcurrentDictionary<int, ViolatedPolicyCollection> PolicyViolations { get; set; } =
			new ConcurrentDictionary<int, ViolatedPolicyCollection>();


		private void SastReportOutput(IOutputTransaction trx, ScanDescriptor scan)
		{

			_log.Debug($"Retrieving XML Report for scan {scan.ScanId}");
			try
			{
				using (var report = CxSastXmlReport.GetXmlReport(RestContext,
					CancelToken, scan.ScanId))
				{
					_log.Debug($"XML Report for scan {scan.ScanId} retrieved.");
					ProcessSASTReport(trx, scan, report);
					_log.Debug($"XML Report for scan {scan.ScanId} processed.");
				}

				OutputSASTScanSummary(trx, scan);
			}
			catch (AggregateException aex)
			{
				_log.Warn($"Multiple exceptions caught attempting to retrieve the SAST XML report for {scan.ScanId}" +
					$" in project {scan.Project.ProjectId}: {scan.Project.ProjectName}. ");

				_log.Warn("BEGIN exception report");

				int exCount = 0;

				aex.Handle((x) =>
				{
					_log.Warn($"Exception #{++exCount}", x);

					return true;
				});

				_log.Warn("END exception report");
			}
			catch (Exception ex)
			{
				_log.Warn($"Error attempting to retrieve the SAST XML report for {scan.ScanId}" +
					$" in project {scan.Project.ProjectId}: {scan.Project.ProjectName}. ", ex);
			}
		}

		private void ScaReportOutput(IOutputTransaction trx, ScanDescriptor sd)
		{
			Dictionary<String, CxOsaLicenses.License> licenseIndex =
				new Dictionary<string, CxOsaLicenses.License>();

			Dictionary<String, int> licenseCount =
				new Dictionary<string, int>();

			try
			{
				var licenses = CxOsaLicenses.GetLicenses(RestContext, CancelToken, sd.ScanId);

				foreach (var l in licenses)
				{
					licenseIndex.Add(l.LicenseId, l);

					if (licenseCount.ContainsKey(l.RiskLevel))
						licenseCount[l.RiskLevel]++;
					else
						licenseCount.Add(l.RiskLevel, 1);
				}
			}
			catch (Exception ex)
			{
				_log.Warn($"Could not obtain license data for scan {sd.ScanId} in project " +
					$"{sd.Project.ProjectId}: {sd.Project.ProjectName}.  License data will not be" +
					$" available.", ex);
			}

			Dictionary<String, CxOsaLibraries.Library> libraryIndex =
			new Dictionary<string, CxOsaLibraries.Library>();


			try
			{
				var libraries = CxOsaLibraries.GetLibraries(RestContext, CancelToken, sd.ScanId);

				foreach (var lib in libraries)
					libraryIndex.Add(lib.LibraryId, lib);
			}
			catch (Exception ex)
			{
				_log.Warn($"Could not obtain library data for scan {sd.ScanId} in project " +
					$"{sd.Project.ProjectId}: {sd.Project.ProjectName}.  Library data will not be" +
					$" available.", ex);
			}

			OutputScaScanSummary(trx, sd, licenseCount);

			OutputScaScanDetails(trx, sd, licenseIndex, libraryIndex);

		}

		private void OutputScaScanDetails(IOutputTransaction trx, ScanDescriptor sd, Dictionary<string, CxOsaLicenses.License> licenseIndex,
			Dictionary<string, CxOsaLibraries.Library> libraryIndex)
		{
			try
			{
				var vulns = CxOsaVulnerabilities.GetVulnerabilities(RestContext,
					CancelToken, sd.ScanId);

				var header = new SortedDictionary<String, Object>();
				AddPrimaryKeyElements(sd.Project, header);
				header.Add(PropertyKeys.KEY_SCANFINISH, sd.FinishedStamp);

				foreach (var vuln in vulns)
				{
					var flat = new SortedDictionary<String, Object>(header);

					flat.Add(PropertyKeys.KEY_SCANID, sd.ScanId);

					flat.Add("VulnerabilityId", vuln.VulerabilityId);
					flat.Add(PropertyKeys.KEY_SIMILARITYID, vuln.SimilarityId);
					flat.Add("CVEName", vuln.CVEName);
					flat.Add("CVEDescription", vuln.CVEDescription);
					flat.Add("CVEUrl", vuln.CVEUrl);
					flat.Add("CVEPubDate", vuln.CVEPublishDate);
					flat.Add("CVEScore", vuln.CVEScore);
					flat.Add("Recommendation", vuln.Recommendations);
					flat.Add(PropertyKeys.KEY_SCANRISKSEV, vuln.Severity.Name);
					flat.Add("State", vuln.State.StateName);


					flat.Add("LibraryId", vuln.LibraryId);

					var lib = libraryIndex[vuln.LibraryId];
					if (lib != null)
					{
						flat.Add("LibraryName", lib.LibraryName);
						flat.Add("LibraryVersion", lib.LibraryVersion);
						flat.Add("LibraryReleaseDate", lib.ReleaseDate);
						flat.Add("LibraryLatestVersion", lib.LatestVersion);
						flat.Add("LibraryLatestReleaseDate", lib.LatestVersionReleased);
					}

					StringBuilder licenseStr = new StringBuilder();

					foreach (var license in lib.Licenses)
					{
						if (licenseStr.Length > 0)
							licenseStr.Append(";");
						licenseStr.Append(licenseIndex[license].LicenseName);

						flat.Add($"LibraryLegalRisk_{licenseIndex[license].LicenseName.Replace(" ", "")}",
							licenseIndex[license].RiskLevel);
					}

					flat.Add("LibraryLicenses", licenseStr.ToString());

					trx.write(ScaScanDetailOut, flat);
				}
			}
			catch (Exception ex)
			{
				_log.Warn($"Could not obtain vulnerability data for scan {sd.ScanId} in project " +
				$"{sd.Project.ProjectId}: {sd.Project.ProjectName}.  Vulnerability data will not be" +
				$" available.", ex);
			}
		}

		private void OutputScaScanSummary(IOutputTransaction trx, ScanDescriptor sd, Dictionary<string, int> licenseCount)
		{
			var flat = new SortedDictionary<String, Object>();
			AddPrimaryKeyElements(sd.Project, flat);
			AddPolicyViolationProperties(sd, flat);
			flat.Add(PropertyKeys.KEY_SCANID, sd.ScanId);
			flat.Add(PropertyKeys.KEY_SCANSTART, ScaScanCache[sd.ScanId].StartTime);
			flat.Add(PropertyKeys.KEY_SCANFINISH, ScaScanCache[sd.ScanId].FinishTime);

			foreach (var k in licenseCount.Keys)
				flat.Add($"Legal{k}", licenseCount[k]);


			try
			{
				var summary = CxOsaSummaryReport.GetReport(RestContext, CancelToken, sd.ScanId);

				flat.Add("HighVulnerabilityLibraries", summary.HighVulnerabilityLibraries);
				flat.Add("LowVulnerabilityLibraries", summary.LowVulnerabilityLibraries);
				flat.Add("MediumVulnerabilityLibraries", summary.MediumVulnerabilityLibraries);
				flat.Add("NonVulnerableLibraries", summary.NonVulnerableLibraries);
				flat.Add("TotalHighVulnerabilities", summary.TotalHighVulnerabilities);
				flat.Add("TotalLibraries", summary.TotalLibraries);
				flat.Add("TotalLowVulnerabilities", summary.TotalLowVulnerabilities);
				flat.Add("TotalMediumVulnerabilities", summary.TotalMediumVulnerabilities);
				flat.Add("VulnerabilityScore", summary.VulnerabilityScore);
				flat.Add("VulnerableAndOutdated", summary.VulnerableAndOutdated);
				flat.Add("VulnerableAndUpdated", summary.VulnerableAndUpdated);
			}
			catch (Exception ex)
			{
				_log.Warn($"Error obtaining summary report for SCA scan {sd.ScanId} " +
					$"in project {sd.Project.ProjectName}", ex);
			}

			trx.write(ScaScanSummaryOut, flat);
		}

		private Transformer(CxRestContext ctx, CancellationToken token,
			String previousStatePath, IProjectFilter filter)
		{
			RestContext = ctx;
			CancelToken = token;
			Filter = filter;
			_state = new CrawlState(previousStatePath);

			ResolveScans().Wait();

		}


		private async Task ResolveScans()
		{
			var policyTask = PopulatePolicies();

			var presetsTask = PopulatePresets();

			var teamsTask = PopulateTeams();

			_log.Debug("Resolving projects.");

			var projects = await Task.Run(() => CxProjects.GetProjects(RestContext, CancelToken), CancelToken);

			Policies = await policyTask;
			Teams = await teamsTask;
			Presets = await presetsTask;


			Parallel.ForEach(projects, new ParallelOptions { CancellationToken = CancelToken }, (p) =>
			{

				if (p.ProjectName == null)
					return;

				String combinedPolicyNames = String.Empty;

				if (Policies != null)
				{
					try
					{
						IEnumerable<int> projectPolicyList = CxMnoPolicies.GetPolicyIdsForProject
							(RestContext, CancelToken, p.ProjectId);

						if (projectPolicyList != null)
						{
							Policies.CorrelateProjectToPolicies(p.ProjectId, projectPolicyList);
							combinedPolicyNames = GetFlatPolicyNames(Policies, projectPolicyList);
						}
					}
					catch (Exception ex)
					{
						_log.Warn($"Unable to correlate policies to project {p.ProjectId}: {p.ProjectName}. " +
							$"Policy statistics will be unavalable.", ex);
					}
				}

				var cfDict = new SortedDictionary<String, String>();
				p.CustomFields.ForEach((cf) => cfDict.Add(cf.FieldName, cf.FieldValue));


				// Load the projects
				String teamName = Teams.ContainsKey(p.TeamId) ? Teams[p.TeamId] : String.Empty;
				if (String.Empty == teamName)
				{
					_log.ErrorFormat("Unable to find a team name for team id [{0}] when adding project {1}:{2}", p.TeamId,
						p.ProjectId, p.ProjectName);

					return;
				}

				String presetName = Presets.ContainsKey(p.PresetId) ? Presets[p.PresetId] : String.Empty;
				if (String.Empty == presetName)
				{
					_log.ErrorFormat("Unable to find a preset name for preset id [{0}] " +
						"when adding project {1}:{2}; project may be assigned an invalid preset.", p.PresetId,
						p.ProjectId, p.ProjectName);

					return;
				}


				if (!Filter.Matches(teamName, p.ProjectName))
				{
					if (_log.IsDebugEnabled)
						_log.Debug($"FILTERED: Team: [{teamName}] Project: [{p.ProjectName}]");

					return;
				}

				if (!_loadedProjects.TryAdd(p.ProjectId,
					new ProjectDescriptor()
					{
						ProjectId = p.ProjectId,
						ProjectName = p.ProjectName,
						TeamName = teamName,
						TeamId = p.TeamId,
						PresetId = p.PresetId,
						PresetName = presetName,
						Policies = combinedPolicyNames,
						CustomFields = cfDict
					}
				))
				{
					_log.WarnFormat("Rejected changed when adding new project with duplicate id {0}: New name: [{1}] current name: [{2}].",
						p.ProjectId, p.ProjectName, _loadedProjects[p.ProjectId].ProjectName);

					return;
				}

			});


			// _loadedProjects has a collection of projects loaded from the SAST system
			_state.ConfirmProjects(new List<ProjectDescriptor>(_loadedProjects.Values));

			_log.Info($"{_state.ProjectCount} projects are targets to check for new scans. Since last scan: {_state.DeletedProjects}"
				+ $" projects removed, {_state.NewProjects} new projects.");


			_log.Debug("Resolving scans.");

			// Load the scans for each project
			Parallel.ForEach(_state.Projects, new ParallelOptions { CancellationToken = CancelToken },
				(p) =>
				{

					// SAST Scans
					var sastScans = CxSastScans.GetScans(RestContext, CancelToken, CxSastScans.ScanStatus.Finished, p.ProjectId);

					foreach (var s in sastScans)
					{
						// Add to crawl state.
						if (_log.IsTraceEnabled())
							_log.Trace($"SAST scan record: {s}");
						_state.AddScan(s.ProjectId, s.ScanType, ScanProductType.SAST, s.ScanId, s.FinishTime, s.Engine);
						SastScanCache.TryAdd(s.ScanId, s);
					}

					// OSA scans
					var osaScans = CxOsaScans.GetScans(RestContext, CancelToken, p.ProjectId);
					foreach (var s in osaScans)
					{
						// Add to crawl state.
						if (_log.IsTraceEnabled())
							_log.Trace($"OSA scan record: {s}");
						_state.AddScan(s.ProjectId, "Composition", ScanProductType.SCA, s.ScanId, s.FinishTime, "N/A");
						ScaScanCache.TryAdd(s.ScanId, s);
					}


				});
		}


		private async Task<ConcurrentDictionary<String, String>> PopulateTeams()
		{
			return await Task.Run(() =>
			{

				_log.Debug("Retrieving teams.");

				ConcurrentDictionary<String, String> retVal = new ConcurrentDictionary<string, string>();

				var teamEnum = CxTeams.GetTeams(RestContext, CancelToken);

				foreach (var team in teamEnum)
				{
					if (String.IsNullOrEmpty(team.TeamId) || String.IsNullOrEmpty(team.TeamName))
						continue;

					if (!retVal.TryAdd(team.TeamId, team.TeamName))
						retVal[team.TeamId] = team.TeamName;
				}

				return retVal;

			}, CancelToken);
		}

		private async Task<ConcurrentDictionary<int, String>> PopulatePresets()
		{
			return await Task.Run(() =>
		   {
			   _log.Debug("Retrieving presets.");

			   ConcurrentDictionary<int, String> retVal = new ConcurrentDictionary<int, string>();

			   var presetEnum = CxPresets.GetPresets(RestContext, CancelToken);

			   foreach (var preset in presetEnum)
			   {
				   if (preset.PresetName == null)
					   continue;

				   if (!retVal.TryAdd(preset.PresetId, preset.PresetName))
					   retVal[preset.PresetId] = preset.PresetName;
			   }

			   return retVal;
		   }, CancelToken);
		}

		private async Task<ProjectPolicyIndex> PopulatePolicies()
		{
			// Policies may not have data if M&O is not installed.
			try
			{
				return await Task.Run(
					() =>
					{
						_log.Debug("Retrieving policies, if available.");

						return new ProjectPolicyIndex(CxMnoPolicies.GetAllPolicies(RestContext, CancelToken));

					}, CancelToken);
			}
			catch (Exception ex)
			{
				String msg = "Policy data is not available. M&O was unreachable.  You can omit the M&O URL in the configuration if M&O is not installed.";

				if (_log.IsDebugEnabled)
					_log.Debug(msg, ex);
				else
					_log.Warn(msg);
			}


			return null;
		}




		private void ExecuteSweep()
		{

			if (_state.Projects == null)
			{
				_log.Error("Scans to crawl do not appear to be resolved, unable to crawl scan data.");
				return;
			}


			_log.Info($"Crawling {_state.ScanCount} scans.");


			// Lookup policy violations, report the project information records.
			Parallel.ForEach<ProjectDescriptor>(_state.Projects, ThreadOpts,
			(project) =>
			{
				// Do not output project info if a project has no scans.
				if (_state.GetScanCountForProject(project.ProjectId) <= 0)
				{
					_log.Info($"Project {project.ProjectId}:{project.TeamName}:{project.ProjectName} has no new scans to process.");
					return;
				}

				// Project info is a moment-in-time sample of the state of the project.  This can be output
				// in a transaction context different than the scans.
				using (var pinfoTrx = Output.StartTransaction () )
					if (PolicyViolations.TryAdd(project.ProjectId, new ViolatedPolicyCollection()))
					{
						if (Policies != null)
							try
							{
                                // Collect policy violations, only once per project
                                var violations = CxMnoRetreivePolicyViolations.GetViolations(RestContext, CancelToken, project.ProjectId, Policies);
                                if (violations != null)
                                    PolicyViolations[project.ProjectId] = violations;
                            }
							catch (Exception ex)
							{
								_log.Debug($"Policy violations for project {project.ProjectId}:" +
								  $"{project.ProjectName} are unavailable.", ex);
							}

						OutputProjectInfoRecords(pinfoTrx, project);

						if (!CancelToken.IsCancellationRequested)
							pinfoTrx.Commit();
					}

				// One transaction per scan since the entire set of scan records should be output
				// before the scan date is updated.
				foreach (var scan in _state.GetScansForProject(project.ProjectId))
				{
					if (CancelToken.IsCancellationRequested)
						break;

					using (var scanTrx = Output.StartTransaction())
					{
						// Increment the policy violation stats for each scan.
						scan.IncrementPolicyViolations(PolicyViolations[scan.Project.ProjectId].GetViolatedRulesByScanId(scan.ScanId));

						_log.Info($"Processing {scan.ScanProduct} scan {scan.ScanId}:{scan.Project.ProjectId}:{scan.Project.TeamName}:{scan.Project.ProjectName}[{scan.FinishedStamp}]");

						switch (scan.ScanProduct)
						{
							case ScanProductType.SAST:
								SastReportOutput(scanTrx, scan);
								break;

							case ScanProductType.SCA:
								ScaReportOutput(scanTrx, scan);
								break;

						}

						OutputPolicyViolationDetails(scanTrx, scan);

						// Persist the date of this scan since it has been output.
						if (!CancelToken.IsCancellationRequested && scanTrx.Commit())
							_state.ScanCompleted(scan);
						else
						{
							// Stop processing further scans in this project if the commit
							// for the scan information fails.
							_log.Warn($"Stopped processing scans for project {project.ProjectId}:{project.TeamName}:{project.ProjectName} at {scan.ScanId}, will resume here next crawl.");
							return;
						}
					}
				}

			});


		}



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
		public static void DoTransform(int concurrentThreads, String previousStatePath, String instanceId,
		CxRestContext ctx, IProjectFilter filter, RecordNames records, CancellationToken token)
		{
			Transformer xform = new Transformer(ctx, token, previousStatePath, filter)
			{
				ThreadOpts = new ParallelOptions()
				{
					CancellationToken = token,
					MaxDegreeOfParallelism = concurrentThreads
				},
				ProjectInfoOut = Output.RegisterRecord (records.ProjectInfo),
				SastScanSummaryOut = Output.RegisterRecord(records.SASTScanSummary),
				SastScanDetailOut = Output.RegisterRecord(records.SASTScanDetail),
				PolicyViolationDetailOut = Output.RegisterRecord(records.PolicyViolations),
				ScaScanSummaryOut = Output.RegisterRecord(records.SCAScanSummary),
				ScaScanDetailOut = Output.RegisterRecord(records.SCAScanDetail),
				InstanceId = instanceId

			};

			xform.ExecuteSweep();
		}

		private void OutputPolicyViolationDetails(IOutputTransaction trx, ScanDescriptor scan)
		{
			var header = new SortedDictionary<String, Object>();
			AddPrimaryKeyElements(scan.Project, header);
			header.Add(PropertyKeys.KEY_SCANID, scan.ScanId);
			header.Add(PropertyKeys.KEY_SCANPRODUCT, scan.ScanProduct.ToString());
			header.Add(PropertyKeys.KEY_SCANTYPE, scan.ScanType);

			var violatedRules = PolicyViolations[scan.Project.ProjectId].
				GetViolatedRulesByScanId(scan.ScanId);

			if (violatedRules != null)
				foreach (var rule in violatedRules)
				{
					var flat = new SortedDictionary<String, Object>(header);
					flat.Add("PolicyName", Policies.GetPolicyById(rule.PolicyId).Name);
					flat.Add("RuleId", rule.RuleId);
					flat.Add("RuleName", rule.Name);
					flat.Add("RuleDescription", rule.Description);
					flat.Add("RuleType", rule.RuleType);
					flat.Add("RuleCreateDate", rule.CreatedOn);
					flat.Add("FirstViolationDetectionDate", rule.FirstDetectionDate);
					flat.Add("ViolationName", rule.ViolationName);
					if (rule.ViolationOccured.HasValue)
						flat.Add("ViolationOccurredDate", rule.ViolationOccured.Value);
					if (rule.ViolationRiskScore.HasValue)
						flat.Add("ViolationRiskScore", rule.ViolationRiskScore.Value);
					flat.Add("ViolationSeverity", rule.ViolationSeverity);
					if (rule.ViolationSource != null)
						flat.Add("ViolationSource", rule.ViolationSource);
					flat.Add("ViolationState", rule.ViolationState);
					flat.Add("ViolationStatus", rule.ViolationStatus);
					flat.Add("ViolationId", rule.ViolationId);
					if (rule.ViolationType != null)
						flat.Add("ViolationType", rule.ViolationType);

					trx.write(PolicyViolationDetailOut, flat);
				}
		}

		private void ProcessSASTReport(IOutputTransaction trx, ScanDescriptor scan, Stream report)
		{
			var reportRec = new SortedDictionary<String, Object>();
			AddPrimaryKeyElements(scan.Project, reportRec);
			reportRec.Add(PropertyKeys.KEY_SCANID, scan.ScanId);
			reportRec.Add(PropertyKeys.KEY_SCANPRODUCT, scan.ScanProduct.ToString ());
			reportRec.Add(PropertyKeys.KEY_SCANTYPE, scan.ScanType);
			reportRec.Add(PropertyKeys.KEY_SCANFINISH, scan.FinishedStamp);

			SortedDictionary<String, Object> curResultRec = null;
			SortedDictionary<String, Object> curQueryRec = null;
			SortedDictionary<String, Object> curPath = null;
			SortedDictionary<String, Object> curPathNode = null;
			bool inSnippet = false;

			using (XmlReader xr = XmlReader.Create(report))
			{
				while (xr.Read())
				{
					if (xr.NodeType == XmlNodeType.Element)
					{
						if (xr.Name.CompareTo("CxXMLResults") == 0)
						{
							_log.Trace($"[Scan: {scan.ScanId}] Processing attributes in CxXMLResults.");

							scan.Preset = xr.GetAttribute("Preset");
							scan.Initiator = xr.GetAttribute("InitiatorName");
							scan.DeepLink = xr.GetAttribute("DeepLink");
							scan.ScanTime = xr.GetAttribute("ScanTime");
							scan.ReportCreateTime = DateTime.Parse(xr.GetAttribute("ReportCreationTime"));
							scan.Comments = xr.GetAttribute("ScanComments");
							scan.SourceOrigin = xr.GetAttribute("SourceOrigin");
							continue;
						}

						if (xr.Name.CompareTo("Query") == 0)
						{
							_log.Trace($"[Scan: {scan.ScanId}] Processing attributes in Query " +
								$"[{xr.GetAttribute("id")} - {xr.GetAttribute("name")}].");

							curQueryRec = new SortedDictionary<String, Object>
								(reportRec);

							curQueryRec.Add("QueryCategories", xr.GetAttribute("categories"));
							curQueryRec.Add("QueryId", xr.GetAttribute("id"));
							curQueryRec.Add("QueryCweId", xr.GetAttribute("cweId"));
							curQueryRec.Add("QueryName", xr.GetAttribute("name"));
							curQueryRec.Add("QueryGroup", xr.GetAttribute("group"));
							curQueryRec.Add("QuerySeverity", xr.GetAttribute("Severity"));
							curQueryRec.Add("QueryLanguage", xr.GetAttribute("Language"));
							curQueryRec.Add("QueryVersionCode", xr.GetAttribute("QueryVersionCode"));
							continue;
						}

						if (xr.Name.CompareTo("Result") == 0)
						{
							_log.Trace($"[Scan: {scan.ScanId}] Processing attributes in Result " +
								$"[{xr.GetAttribute("NodeId")}].");

							scan.IncrementSeverity(xr.GetAttribute("Severity"));

							curResultRec = new SortedDictionary<String, Object>(curQueryRec);
							curResultRec.Add("VulnerabilityId", xr.GetAttribute("NodeId"));
							curResultRec.Add("SinkFileName", xr.GetAttribute("FileName"));
							curResultRec.Add("Status", xr.GetAttribute("Status"));
							curResultRec.Add("SinkLine", xr.GetAttribute("Line"));
							curResultRec.Add("SinkColumn", xr.GetAttribute("Column"));
							curResultRec.Add("FalsePositive", xr.GetAttribute("FalsePositive"));
							curResultRec.Add("ResultSeverity", xr.GetAttribute("Severity"));
							// TODO: Translate state number to an appropriate string
							curResultRec.Add("State", xr.GetAttribute("state"));
							curResultRec.Add("Remark", xr.GetAttribute("Remark"));
							curResultRec.Add("ResultDeepLink", xr.GetAttribute("DeepLink"));

							try
							{
								curResultRec.Add("FirstDetectionDate", DateTime.Parse(xr.GetAttribute("DetectionDate")) );
							}
							catch (Exception)
							{
								// Don't output it if there is a parse exception.
							}

							continue;
						}

						if (xr.Name.CompareTo("Path") == 0)
						{
							curPath = new SortedDictionary<String, Object>(curResultRec);
							curPath.Add("ResultId", xr.GetAttribute("ResultId"));
							curPath.Add("PathId", xr.GetAttribute("PathId"));
							curPath.Add(PropertyKeys.KEY_SIMILARITYID, xr.GetAttribute("SimilarityId"));
							continue;
						}

						if (xr.Name.CompareTo("PathNode") == 0)
						{
							curPathNode = new SortedDictionary<String, Object>(curPath);
							continue;
						}

						if (xr.Name.CompareTo("FileName") == 0 && curPathNode != null)
						{
							curPathNode.Add("NodeFileName", xr.ReadElementContentAsString());
							continue;
						}

						if (xr.Name.CompareTo("Line") == 0 && curPathNode != null && !inSnippet)
						{
							curPathNode.Add("NodeLine", xr.ReadElementContentAsString());
							continue;
						}

						if (xr.Name.CompareTo("Column") == 0 && curPathNode != null)
						{
							curPathNode.Add("NodeColumn", xr.ReadElementContentAsString());
							continue;
						}

						if (xr.Name.CompareTo("NodeId") == 0 && curPathNode != null)
						{
							curPathNode.Add("NodeId", xr.ReadElementContentAsString());
							continue;
						}

						if (xr.Name.CompareTo("Name") == 0 && curPathNode != null)
						{
							curPathNode.Add("NodeName", xr.ReadElementContentAsString());
							continue;
						}

						if (xr.Name.CompareTo("Type") == 0 && curPathNode != null)
						{
							curPathNode.Add("NodeType", xr.ReadElementContentAsString());
							continue;
						}

						if (xr.Name.CompareTo("Length") == 0 && curPathNode != null)
						{
							curPathNode.Add("NodeLength", xr.ReadElementContentAsString());
							continue;
						}

						if (xr.Name.CompareTo("Snippet") == 0 && curPathNode != null)
						{
							inSnippet = true;
							continue;
						}

						if (xr.Name.CompareTo("Code") == 0 && curPathNode != null)
						{
							curPathNode.Add("NodeCodeSnippet", xr.ReadElementContentAsString());
							continue;
						}
					}


					if (xr.NodeType == XmlNodeType.EndElement)
					{
						if (xr.Name.CompareTo("CxXMLResults") == 0)
						{
							_log.Trace($"[Scan: {scan.ScanId}] Finished processing CxXMLResults");
							continue;
						}

						if (xr.Name.CompareTo("Query") == 0)
						{
							curQueryRec = null;
							continue;
						}

						if (xr.Name.CompareTo("Result") == 0)
						{
							curResultRec = null;
							continue;
						}

						if (xr.Name.CompareTo("Path") == 0)
						{
							curPath = null;
							continue;
						}

						if (xr.Name.CompareTo("PathNode") == 0)
						{
							trx.write(SastScanDetailOut, curPathNode);
							curPathNode = null;
							continue;
						}

						if (xr.Name.CompareTo("Snippet") == 0)
						{
							inSnippet = false;
							continue;
						}
					}
				}
			}
		}

		private void OutputSASTScanSummary(IOutputTransaction trx, ScanDescriptor scanRecord)
		{
			if (SastScanSummaryOut == null)
				return;

			var flat = new SortedDictionary<String, Object>();
			AddPrimaryKeyElements(scanRecord.Project, flat);
			flat.Add(PropertyKeys.KEY_SCANID, scanRecord.ScanId);
			flat.Add(PropertyKeys.KEY_SCANPRODUCT, scanRecord.ScanProduct.ToString());
			flat.Add(PropertyKeys.KEY_SCANTYPE, scanRecord.ScanType);
			flat.Add(PropertyKeys.KEY_SCANFINISH, scanRecord.FinishedStamp);
			flat.Add(PropertyKeys.KEY_SCANSTART, SastScanCache[scanRecord.ScanId].StartTime);
			flat.Add(PropertyKeys.KEY_ENGINESTART, SastScanCache[scanRecord.ScanId].EngineStartTime);
			flat.Add(PropertyKeys.KEY_ENGINEFINISH, SastScanCache[scanRecord.ScanId].EngineFinishTime);
			flat.Add(PropertyKeys.KEY_SCANRISK, SastScanCache[scanRecord.ScanId].ScanRisk);
			flat.Add(PropertyKeys.KEY_SCANRISKSEV, SastScanCache[scanRecord.ScanId].ScanRiskSeverity);
			flat.Add("LinesOfCode", SastScanCache[scanRecord.ScanId].LinesOfCode);
			flat.Add("FailedLinesOfCode", SastScanCache[scanRecord.ScanId].FailedLinesOfCode);
			flat.Add("FileCount", SastScanCache[scanRecord.ScanId].FileCount);
			flat.Add("CxVersion", SastScanCache[scanRecord.ScanId].CxVersion);
			flat.Add("Languages", SastScanCache[scanRecord.ScanId].Languages);
			flat.Add(PropertyKeys.KEY_PRESET, scanRecord.Preset);
			flat.Add("Initiator", scanRecord.Initiator);
			flat.Add("DeepLink", scanRecord.DeepLink);
			flat.Add("ScanTime", scanRecord.ScanTime);
			flat.Add("ReportCreationTime", scanRecord.ReportCreateTime);
			flat.Add("ScanComments", scanRecord.Comments);
			flat.Add("SourceOrigin", scanRecord.SourceOrigin);
			flat.Add("ScanProcessingEngine", scanRecord.Engine);
			foreach (var sev in scanRecord.SeverityCounts.Keys)
				flat.Add(sev, scanRecord.SeverityCounts[sev]);

			AddPolicyViolationProperties(scanRecord, flat);

			trx.write(SastScanSummaryOut, flat);
		}

		private static void AddPolicyViolationProperties(ScanDescriptor scanRecord,
			IDictionary<String, Object> rec)
		{
			if (scanRecord.HasPoliciesApplied)
			{
				rec.Add("PoliciesViolated", scanRecord.PoliciesViolated);
				rec.Add("RulesViolated", scanRecord.RulesViolated);
				rec.Add("PolicyViolations", scanRecord.Violations);
			}
		}

		private void OutputProjectInfoRecords(IOutputTransaction trx, ProjectDescriptor project)
		{
			var flat = new SortedDictionary<String, Object>();
			AddPrimaryKeyElements(project, flat);

			flat.Add("LastCrawlDate", DateTime.Now);

			flat.Add(PropertyKeys.KEY_PRESET, project.PresetName);
			flat.Add("Policies", project.Policies);

			foreach (var lastScanProduct in project.LatestScanDateByProduct.Keys)
				flat.Add($"{lastScanProduct}_LastScanDate",
					project.LatestScanDateByProduct[lastScanProduct]);

			foreach (var scanCountProduct in project.ScanCountByProduct.Keys)
				flat.Add($"{scanCountProduct}_Scans",
					project.ScanCountByProduct[scanCountProduct]);

			if (project.CustomFields != null && project.CustomFields.Count > 0)
				flat.Add(PropertyKeys.KEY_CUSTOMFIELDS, project.CustomFields);

			trx.write(ProjectInfoOut, flat);
		}

		private void AddPrimaryKeyElements(ProjectDescriptor rec, IDictionary<String, Object> flat)
		{
			flat.Add(PropertyKeys.KEY_PROJECTID, rec.ProjectId);
			flat.Add(PropertyKeys.KEY_PROJECTNAME, rec.ProjectName);
			flat.Add(PropertyKeys.KEY_TEAMNAME, rec.TeamName);
			if (!String.IsNullOrEmpty(InstanceId))
				flat.Add(PropertyKeys.KEY_INSTANCEID, InstanceId);
		}

		private static String GetFlatPolicyNames(PolicyCollection policies,
			IEnumerable<int> policyIds)
		{
			StringBuilder b = new StringBuilder();

			foreach (var pid in policyIds)
			{
				if (b.Length > 0)
					b.Append(';');

				b.Append(policies.GetPolicyById(pid).Name);
			}

			return b.ToString();
		}
	}
}
