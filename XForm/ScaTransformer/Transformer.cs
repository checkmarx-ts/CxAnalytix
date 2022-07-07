using CxAnalytix.Configuration.Impls;
using CxAnalytix.XForm.Common;
using CxAnalytix.XForm.ScaTransformer.Config;
using CxRestClient;
using CxRestClient.SCA;
using log4net;
using System;
using System.Threading;
using System.Threading.Tasks;
using System.Linq;
using System.Collections.Generic;
using SDK.Modules.Transformer.Data;
using System.Collections.Concurrent;
using CxAnalytix.Extensions;
using static SDK.Modules.Transformer.Data.ScanDescriptor;
using OutputBootstrapper;
using CxAnalytix.XForm.ScaTransformer.Policy;

namespace CxAnalytix.XForm.ScaTransformer
{
	public class Transformer : BaseTransformer
	{
        private static readonly ILog _log = LogManager.GetLogger(typeof(Transformer));
        private static readonly String STATE_STORAGE_FILE = "CxAnalytixExportState_SCA.json";
		private static readonly String MODULE_NAME = "SCA";

        private Task<ScaPolicyIndex> PoliciesTask { get; set; }
        private ScaPolicyIndex Policies => PoliciesTask.Result;


        public Transformer() : base(MODULE_NAME, typeof(Transformer), STATE_STORAGE_FILE)
		{
		}

		public override string DisplayName => MODULE_NAME;


        private async Task<ScaPolicyIndex> PopulatePolicies(CxSCARestContext ctx)
        {
            return await Task.Run(
                () =>
                {
                    _log.Debug("Retrieving SCA policies.");

                    return new ScaPolicyIndex(CxPolicies.GetPolicies(ctx, ThreadOpts.CancellationToken));
                }, ThreadOpts.CancellationToken);
        }

        public override void DoTransform(CancellationToken token)
		{
			ThreadOpts.CancellationToken = token;

            var apiConCfg = Configuration.Impls.Config.GetConfig<CxScaConnection>();
			var loginCreds = Configuration.Impls.Config.GetConfig<CxMultiTenantCredentials>();

            var restBuilder = new CxSCARestContext.CxSCARestContextBuilder();

            restBuilder
            .WithApiURL(apiConCfg.URL)
            .WithLoginUrl(apiConCfg.LoginURL)
            .WithTenant(loginCreds.Tenant)
            .WithUsername(loginCreds.Username)
            .WithPassword(loginCreds.Password)
            .WithOpTimeout(apiConCfg.TimeoutSeconds)
            .WithSSLValidate(apiConCfg.ValidateCertificates)
            .WithRetryLoop(apiConCfg.RetryLoop);

            var ctx = restBuilder.Build();

            PoliciesTask = PopulatePolicies(ctx);

            var projects = CxProjects.GetProjects(ctx, ThreadOpts.CancellationToken);

            var projectDescriptors = new ConcurrentBag<ProjectDescriptor>();


            _log.Debug("Resolving projects.");


            Parallel.ForEach(projects, ThreadOpts, (p) => {

                // Projects don't need to have a team assignment, unlike in SAST
                if (! ( (p.Teams.Count == 0) ? Filter.Matches(p.ProjectName) : p.Teams.Any((t) => Filter.Matches(t, p.ProjectName))))
                {
                    if (_log.IsDebugEnabled)
                        _log.Debug($"FILTERED: Project: [{p.ProjectName}] with assigned teams [{String.Join(",", p.Teams)}]");
                    return;
                }
                else
                {
                    projectDescriptors.Add(new ProjectDescriptor()
                    {
                        ProjectId = p.ProjectId,
                        ProjectName = p.ProjectName,
                        TeamName = String.Join(";", p.Teams),
                        Policies = String.Join(";", Policies.GetPoliciesForProject(p.ProjectId).Select((policy) => policy.Name))
                    });
                }
            });

            State.ConfirmProjects(projectDescriptors);

            _log.Info($"{State.ProjectCount} projects are targets to check for new scans. Since last crawl: {State.DeletedProjects}"
                + $" projects removed, {State.NewProjects} new projects.");

            _log.Debug("Resolving scans.");

            Parallel.ForEach(State.Projects, ThreadOpts, (p) => {

                var scans = CxScans.GetCompletedScans(ctx, ThreadOpts.CancellationToken, p.ProjectId);

                foreach (var s in scans)
                {
                    // Add to crawl state.
                    if (_log.IsTraceEnabled())
                        _log.Trace($"SCA scan record: {s}");
                    State.AddScan(Convert.ToString(s.ProjectId), s.Origin, ScanProductType.SCA, s.ScanId, s.Updated, null);
                }

            });

            if (State.Projects == null)
            {
                _log.Error("Scans to crawl do not appear to be resolved, unable to crawl scan data.");
                return;
            }

            _log.Info($"Crawling {State.ScanCount} scans.");


            Parallel.ForEach<ProjectDescriptor>(State.Projects, ThreadOpts,
            (project) => {

                if (State.GetScanCountForProject(project.ProjectId) <= 0)
                {
                    _log.Info($"Project {project.ProjectId}:{project.TeamName}:{project.ProjectName} has no new scans to process.");
                    return;
                }

                using (var pinfoTrx = Output.StartTransaction())
                {
                    // TODO: Project Info output.  Is looking up policies required?

                    OutputProjectInfoRecords(pinfoTrx, project);

                    if (!ThreadOpts.CancellationToken.IsCancellationRequested)
                        pinfoTrx.Commit();
                }


                foreach (var scan in State.GetScansForProject(project.ProjectId))
                {
                    if (ThreadOpts.CancellationToken.IsCancellationRequested)
                        break;

                    using (var scanTrx = Output.StartTransaction())
                    {
                        // TODO: Scan summary
                        // TODO: Scan details
                        // TODO: Policy violations


                        if (!ThreadOpts.CancellationToken.IsCancellationRequested && scanTrx.Commit())
                        {
                            State.ScanCompleted(scan);
                            continue;
                        }

                    }
                }
            });

		}



    }
}
