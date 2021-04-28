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

namespace CxAnalytix.AuditTrails.Crawler
{
	public class AuditTrailCrawler
	{
		private const String STORAGE_FILE = "LastAuditTrailCrawl.json";
		private Dictionary<String, IRecordRef> _outMappings = new Dictionary<string, IRecordRef>();
		private CxAuditTrailTableNameConsts _constsInstance = new CxAuditTrailTableNameConsts();
		private static readonly ILog _log = LogManager.GetLogger(typeof(AuditTrailCrawler));


		private AuditTrailCrawler ()
		{
			ReadSinceDate();
			InitOutputMappings();
		}

		private void InitOutputMappings ()
		{
			var outmap = CxAnalytix.Configuration.Config.GetConfig<CxAuditTrailRecordNameMap>(CxAuditTrailRecordNameMap.SECTION_NAME);

			var fields = typeof(CxAuditTrailTableNameConsts).GetFields();
			Dictionary<String, FieldInfo> fieldLookup = new Dictionary<string, FieldInfo>();
			foreach (var f in fields)
				fieldLookup.Add(f.Name, f);


			foreach (var prop in typeof(CxAuditTrailRecordNameMap).GetProperties() )
			{
				if (fieldLookup.ContainsKey (prop.Name))
					_outMappings.Add(fieldLookup[prop.Name].Name, 
						Output.RegisterRecord(GetPropertyValue<CxAuditTrailRecordNameMap, String>(prop.Name, outmap)));
			}
		}

		private static string StorageFile = Path.Combine(CxAnalytix.Configuration.Config.Service.StateDataStoragePath, STORAGE_FILE);

		private void ReadSinceDate()
		{
			SinceDate = DateTime.UnixEpoch;

			var serializer = JsonSerializer.Create();

			if (File.Exists(StorageFile))
				using (StreamReader sr = new StreamReader(StorageFile))
					SinceDate = serializer.Deserialize<DateTime>(new JsonTextReader(sr));

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

			var supressions = Configuration.Config.GetConfig<CxAuditTrailSupressions>(CxAuditTrailSupressions.SECTION_NAME);

			using (var trx = Output.StartTransaction())
			{
				Parallel.ForEach(typeof(CxAuditTrailTableNameConsts).GetFields(),
					new ParallelOptions
					{
						CancellationToken = token,
						MaxDegreeOfParallelism = Configuration.Config.Service.ConcurrentThreads
					},
					(field) =>
				{
					if (token.IsCancellationRequested)
						return;

					if (GetPropertyValue<CxAuditTrailSupressions, bool>(field.Name, supressions))
					{
						_log.Debug($"{field.Name} logging has been suppressed via configuration.");
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
