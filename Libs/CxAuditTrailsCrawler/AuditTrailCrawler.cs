using CxAnalytix.Interfaces.Outputs;
using System;
using System.Threading;
using System.IO;
using Newtonsoft.Json;
using CxAnalytix.AuditTrails.Crawler.Config;
using CxAnalytix.CxAuditTrails;
using System.Collections.Generic;
using System.Reflection;
using CxAnalytix.Interfaces.Audit;
using log4net;
using OutputBootstrapper;
using System.Threading.Tasks;
using CxAnalytix.Configuration.Impls;

namespace CxAnalytix.AuditTrails.Crawler
{
	public class AuditTrailCrawler
	{
		private const String STORAGE_FILE = "LastAuditTrailCrawl.json";
		private Dictionary<String, IRecordRef> _outMappings = new Dictionary<string, IRecordRef>();
		private CxAuditTrailTableNameConsts _constsInstance = new CxAuditTrailTableNameConsts();
		private static readonly ILog _log = LogManager.GetLogger(typeof(AuditTrailCrawler));
		private string StorageFile;

		private CxAuditTrailRecordNameMap _outmap => CxAnalytix.Configuration.Impls.Config.GetConfig<CxAuditTrailRecordNameMap>();

		private CxAuditTrailSuppressions _suppressions => CxAnalytix.Configuration.Impls.Config.GetConfig<CxAuditTrailSuppressions>();

        private CxAnalytixService Service => CxAnalytix.Configuration.Impls.Config.GetConfig<CxAnalytixService>();

		private AuditTrailCrawler ()
		{
            StorageFile = Path.Combine(Service.StateDataStoragePath, STORAGE_FILE);

            ReadSinceDate();
			InitOutputMappings();
		}

		private void InitOutputMappings ()
		{
			var fields = typeof(CxAuditTrailTableNameConsts).GetFields();
			Dictionary<String, FieldInfo> fieldLookup = new Dictionary<string, FieldInfo>();
			foreach (var f in fields)
				fieldLookup.Add(f.Name, f);


			foreach (var prop in typeof(CxAuditTrailRecordNameMap).GetProperties() )
			{
				if (fieldLookup.ContainsKey (prop.Name))
					_outMappings.Add(fieldLookup[prop.Name].Name, 
						Output.RegisterRecord(GetPropertyValue<CxAuditTrailRecordNameMap, String>(prop.Name, _outmap)));
			}
		}


        private void ReadSinceDate()
		{
			SinceDate = DateTime.UnixEpoch;

			var serializer = JsonSerializer.Create();

			if (File.Exists(StorageFile))
				using (StreamReader sr = new StreamReader(StorageFile))
				using (var jtr = new JsonTextReader(sr))
					SinceDate = serializer.Deserialize<DateTime>(jtr);

		}

		private void WriteSinceDate ()
		{
			var serializer = JsonSerializer.Create();
			using (StreamWriter sw = new StreamWriter(StorageFile))
				serializer.Serialize(sw, DateTime.Now);
		}

		private DateTime SinceDate { get; set; }

		private static TResult GetPropertyValue<T, TResult>(String propName, T inst)
		{
			return (TResult)inst.GetType().InvokeMember(propName, BindingFlags.GetProperty, null, inst, null);
		}
		private void InvokeCrawlMethod(String methodName, IAuditTrailCrawler crawler, IOutputTransaction trx, CancellationToken token)
		{
			var recordRef = _outMappings[methodName];
			crawler.GetType().InvokeMember(methodName, BindingFlags.InvokeMethod, null, crawler, new object[]
				{
					SinceDate,
					trx,
					recordRef
				});
		}

		public static void CrawlAuditTrails (CancellationToken token)
		{
			var crawlInvoker = new AuditTrailCrawler();

			IAuditTrailCrawler crawler = new DBCrawler();
			
			if (crawler.IsDisabled)
				return;

            using (var trx = Output.StartTransaction())
			{
				Parallel.ForEach(typeof(CxAuditTrailTableNameConsts).GetFields(),
					new ParallelOptions
					{
						CancellationToken = token,
                        MaxDegreeOfParallelism = crawlInvoker.Service.ConcurrentThreads
					},
					(field) =>
				{
					if (token.IsCancellationRequested)
						return;

					if (GetPropertyValue<CxAuditTrailSuppressions, bool>(field.Name, crawlInvoker._suppressions))
					{
						_log.Info($"{field.Name} logging has been suppressed via configuration.");
						return;
					}

					crawlInvoker.InvokeCrawlMethod(field.Name, crawler, trx, token);
				});

				if (!token.IsCancellationRequested && trx.Commit () )
					crawlInvoker.WriteSinceDate();
			}

		}
	}
}
