using CxAnalytix.Configuration.Impls;
using CxAnalytix.Interfaces.Outputs;
using CxAnalytix.Interfaces.Transform;
using OutputBootstrapper;
using ProjectFilter;
using SDK.Modules.Transformer;
using SDK.Modules.Transformer.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
        public IProjectFilter Filter { get; private set; }


        public BaseTransformer(string moduleName, Type moduleImplType, String stateStorageFileName)
        : base(moduleName, moduleImplType, stateStorageFileName)
        {
            Filter = new FilterImpl(Config.GetConfig<CxFilter>().TeamRegex,
            Config.GetConfig<CxFilter>().ProjectRegex);

            ProjectInfoOut = Output.RegisterRecord(Service.ProjectInfoRecordName);
            SastScanSummaryOut = Output.RegisterRecord(Service.SASTScanSummaryRecordName);
            SastScanDetailOut = Output.RegisterRecord(Service.SASTScanDetailRecordName);
            PolicyViolationDetailOut = Output.RegisterRecord(Service.PolicyViolationsRecordName);
            ScaScanSummaryOut = Output.RegisterRecord(Service.SCAScanSummaryRecordName);
            ScaScanDetailOut = Output.RegisterRecord(Service.SCAScanDetailRecordName);

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

        protected void AddPrimaryKeyElements(ProjectDescriptor rec, IDictionary<String, Object> flat)
        {
            flat.Add("ProjectId", rec.ProjectId);
            if (rec.ProjectName != null)
                flat.Add("ProjectName", rec.ProjectName);
            if (rec.TeamName != null)
                flat.Add("TeamName", rec.TeamName);
            AddInstanceIdentifier(flat);
        }


        protected void OutputProjectInfoRecords(IOutputTransaction trx, ProjectDescriptor project)
        {
            var flat = new SortedDictionary<String, Object>();
            AddPrimaryKeyElements(project, flat);

            flat.Add("LastCrawlDate", DateTime.Now);

            if (project.PresetName != null)
                flat.Add("Preset", project.PresetName);


            if (project.Policies != null)
                flat.Add("Policies", project.Policies);

            foreach (var lastScanProduct in project.LatestScanDateByProduct.Keys)
                flat.Add($"{lastScanProduct}_LastScanDate",
                    project.LatestScanDateByProduct[lastScanProduct]);

            foreach (var scanCountProduct in project.ScanCountByProduct.Keys)
                flat.Add($"{scanCountProduct}_Scans",
                    project.ScanCountByProduct[scanCountProduct]);

            if (project.CustomFields != null && project.CustomFields.Count > 0)
                flat.Add("CustomFields", project.CustomFields);

            flat.Add("IsBranched", project.IsBranched);
            if (project.IsBranched)
            {
                flat.Add("BranchedAtScanId", project.BranchedAtScanId);
                flat.Add("BranchParentProject", project.BranchParentProject);
            }

            trx.write(ProjectInfoOut, flat);
        }


    }
}
