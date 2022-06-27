using CxAnalytix.Configuration.Impls;
using CxAnalytix.Interfaces.Outputs;
using CxAnalytix.Interfaces.Transform;
using OutputBootstrapper;
using ProjectFilter;
using SDK.Modules.Transformer;
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
    }
}
