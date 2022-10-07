using CxAnalytix.Configuration.Impls;
using CxAnalytix.Interfaces.Outputs;
using CxAnalytix.Interfaces.Transform;
using log4net.Util;
using log4net;
using OutputBootstrapper;
using ProjectFilter;
using SDK.Modules.Transformer;
using SDK.Modules.Transformer.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection.PortableExecutable;

namespace CxAnalytix.XForm.Common
{
    public abstract class BaseTransformer : TransformerModule
    {
        protected static CxAnalytixService Service => Config.GetConfig<CxAnalytixService>();

        protected ParallelOptions ThreadOpts { get; }

        protected CrawlState State { get; private set; }


        public IRecordRef ProjectInfoOut { get; internal set; }
        public IRecordRef SastScanSummaryOut { get; internal set; }
        public IRecordRef SastScanDetailOut { get; internal set; }
        public IRecordRef ScaScanSummaryOut { get; internal set; }
        public IRecordRef ScaScanDetailOut { get; internal set; }
        public IRecordRef PolicyViolationDetailOut { get; internal set; }
        public IRecordRef? ScanStatisticsOut { get; internal set; }

        public IProjectFilter Filter { get; private set; }

        private static readonly ILog _log = LogManager.GetLogger(typeof(BaseTransformer));


        public BaseTransformer(string moduleName, Type moduleImplType, String stateStorageFileName)
        : base(moduleName, moduleImplType, stateStorageFileName)
        {
            _log.Debug($"Creating new transformer instance for {moduleName} of type {moduleImplType}");

            Filter = new FilterImpl(Config.GetConfig<CxFilter>().TeamRegex,
            Config.GetConfig<CxFilter>().ProjectRegex);

            ProjectInfoOut = Output.RegisterRecord(Service.ProjectInfoRecordName);
            SastScanSummaryOut = Output.RegisterRecord(Service.SASTScanSummaryRecordName);
            SastScanDetailOut = Output.RegisterRecord(Service.SASTScanDetailRecordName);
            PolicyViolationDetailOut = Output.RegisterRecord(Service.PolicyViolationsRecordName);
            ScaScanSummaryOut = Output.RegisterRecord(Service.SCAScanSummaryRecordName);
            ScaScanDetailOut = Output.RegisterRecord(Service.SCAScanDetailRecordName);

            if (Service.ScanStatisticsRecordName != null && !String.IsNullOrEmpty(Service.ScanStatisticsRecordName))
                ScanStatisticsOut = Output.RegisterRecord(Service.ScanStatisticsRecordName);
            else
                ScanStatisticsOut = null;


            ThreadOpts = new ParallelOptions()
            {
                MaxDegreeOfParallelism = Service.ConcurrentThreads
            };

            State = new CrawlState(Service.StateDataStoragePath, StateStorageFilename);
        }


        protected void AddInstanceIdentifier(IDictionary<String, Object> flat)
        {
            if (!String.IsNullOrEmpty(Service.InstanceIdentifier))
                flat.Add("InstanceId", Service.InstanceIdentifier);
        }

        protected void AddProjectHeaderElements(ProjectDescriptor rec, IDictionary<String, Object> flat)
        {
            flat.Add("ProjectId", rec.ProjectId);
            if (rec.ProjectName != null)
                flat.Add("ProjectName", rec.ProjectName);
            if (rec.TeamName != null)
                flat.Add("TeamName", rec.TeamName);
            AddInstanceIdentifier(flat);
        }

        protected void AddScanHeaderElements(ScanDescriptor rec, IDictionary<String, Object> flat)
        {
            if (rec.Project != null)
                AddProjectHeaderElements(rec.Project, flat);

            if (rec.ScanId != null)
                flat.Add("ScanId", rec.ScanId);

            if (rec.ScanProduct != null)
                flat.Add("ScanProduct", rec.ScanProduct.ToString());

            if (rec.ScanType != null)
                flat.Add("ScanType", rec.ScanType);
        }


        protected virtual void AddAdditionalProjectInfo(IDictionary<String, Object> here, String projectId)
        {

        }

        protected virtual void AddProductsLastScanDateFields(IDictionary<String, Object> here, ProjectDescriptor project)
        {
            foreach (var lastScanProduct in project.LatestScanDateByProduct.Keys)
                here.Add($"{lastScanProduct}_LastScanDate",
                    project.LatestScanDateByProduct[lastScanProduct]);
        }

        protected virtual void AddProductsScanCountFields(IDictionary<String, Object> here, ProjectDescriptor project)
        {
            foreach (var scanCountProduct in project.ScanCountByProduct.Keys)
                here.Add($"{scanCountProduct}_Scans",
                    project.ScanCountByProduct[scanCountProduct]);
        }

        protected void OutputProjectInfoRecords(IOutputTransaction trx, ProjectDescriptor project)
        {
            var flat = new SortedDictionary<String, Object>();
            AddProjectHeaderElements(project, flat);

            flat.Add("LastCrawlDate", DateTime.Now);

            if (project.PresetName != null)
                flat.Add("Preset", project.PresetName);


            if (project.Policies != null)
                flat.Add("Policies", project.Policies);

            AddProductsLastScanDateFields(flat, project);
            AddProductsScanCountFields(flat, project);

            if (project.CustomFields != null && project.CustomFields.Count > 0)
                flat.Add("CustomFields", project.CustomFields);

            flat.Add("IsBranched", project.IsBranched);
            if (project.IsBranched)
            {
                flat.Add("BranchedAtScanId", project.BranchedAtScanId);
                flat.Add("BranchParentProject", project.BranchParentProject);
            }

            AddAdditionalProjectInfo(flat, project.ProjectId);

            trx.write(ProjectInfoOut, flat);
        }


    }
}
