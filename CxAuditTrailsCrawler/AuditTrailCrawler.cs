using CxAnalytix.Interfaces.Outputs;
using System;
using System.Threading;
using System.IO;
using Newtonsoft.Json;
using CxAnalytix.AuditTrails.Crawler.Config;
using CxAnalytix.CxAuditTrails;
using System.Collections.Generic;
using System.Reflection;
using System.Linq;
using CxAnalytix.Interfaces.Audit;

namespace CxAnalytix.AuditTrails.Crawler
{
	public class AuditTrailCrawler
	{
		private const String STORAGE_FILE = "LastAuditTrailCrawl.json";
		private Dictionary<String, IOutput> _outMappings = new Dictionary<string, IOutput>();
		private CxAuditTrailTableNameConsts _constsInstance = new CxAuditTrailTableNameConsts();

		private AuditTrailCrawler (IOutputFactory outFactory)
		{
			InitSinceDate();
			InitOutputMappings(outFactory);
		}

		private void InitOutputMappings (IOutputFactory outFactory)
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
						outFactory.newInstance(GetPropertyValue<CxAuditTrailRecordNameMap, String>(prop.Name, outmap)));
			}
		}

		private void InitSinceDate()
		{
			SinceDate = DateTime.UnixEpoch;

			var storageFile = Path.Combine(CxAnalytix.Configuration.Config.Service.StateDataStoragePath, STORAGE_FILE);

			var serializer = JsonSerializer.Create();

			if (File.Exists(storageFile))
				using (StreamReader sr = new StreamReader(storageFile))
					SinceDate = serializer.Deserialize<DateTime>(new JsonTextReader(sr));

			using (StreamWriter sw = new StreamWriter(storageFile))
				serializer.Serialize(sw, DateTime.Now);
		}

		private DateTime SinceDate { get; set; }

		private static TResult GetPropertyValue<T, TResult>(String propName, T inst)
		{
			return (TResult)inst.GetType().InvokeMember(propName, BindingFlags.GetProperty, null, inst, null);
		}
		private void InvokeCrawlMethod(String methodName, IAuditTrailCrawler crawler, CancellationToken token)
		{
			var outInst = _outMappings[methodName];
			crawler.GetType().InvokeMember(methodName, BindingFlags.InvokeMethod, null, crawler, new object[]
				{
					SinceDate,
					outInst
				});
		}

		public static void CrawlAuditTrails (IOutputFactory outFactory, CancellationToken token)
		{
			var crawlInvoker = new AuditTrailCrawler(outFactory);

			IAuditTrailCrawler crawler = new DBCrawler();
			
			if (crawler.IsDisabled)
				return;

			var supressions = CxAnalytix.Configuration.Config.GetConfig<CxAuditTrailSupressions>(CxAuditTrailSupressions.SECTION_NAME);

			foreach (var field in typeof(CxAuditTrailTableNameConsts).GetFields())
			{
				if (token.IsCancellationRequested)
					break;

				if (GetPropertyValue<CxAuditTrailSupressions, bool>(field.Name, supressions))
					continue;

				crawlInvoker.InvokeCrawlMethod(field.Name, crawler, token);
			}
		}
	}
}
