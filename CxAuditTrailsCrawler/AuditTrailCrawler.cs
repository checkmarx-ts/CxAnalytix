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

namespace CxAnalytix.AuditTrails.Crawler
{
	public class AuditTrailCrawler
	{
		private const String STORAGE_FILE = "LastAuditTrailCrawl.json";
		private Dictionary<String, IOutput> _outMappings = new Dictionary<string, IOutput>();

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

			var valInst = new CxAuditTrailTableNameConsts();

			foreach (var prop in typeof(CxAuditTrailRecordNameMap).GetProperties() )
			{
				if (fieldLookup.ContainsKey (prop.Name))
					_outMappings.Add(fieldLookup[prop.Name].GetValue (valInst) as String, 
						outFactory.newInstance(typeof (CxAuditTrailRecordNameMap).
						InvokeMember(prop.Name, BindingFlags.GetProperty, null, outmap, null) as String));
			}
		}

		private void InitSinceDate()
		{
			SinceDate = DateTime.MinValue;

			var storageFile = Path.Combine(CxAnalytix.Configuration.Config.Service.StateDataStoragePath, STORAGE_FILE);

			var serializer = JsonSerializer.Create();

			if (File.Exists(storageFile))
				using (StreamReader sr = new StreamReader(storageFile))
					SinceDate = serializer.Deserialize<DateTime>(new JsonTextReader(sr));

			using (StreamWriter sw = new StreamWriter(storageFile))
				serializer.Serialize(sw, DateTime.Now);
		}

		private DateTime SinceDate { get; set; }

		private void ExecuteCrawlOnTable(bool supressed, String tableName)
		{
			if (supressed)
				return;



		}

		public static void CrawlAuditTrails (IOutputFactory outFactory, CancellationToken token)
		{
			var logicController = new AuditTrailCrawler(outFactory);

			var supressed = CxAnalytix.Configuration.Config.GetConfig<CxAuditTrailSupressions>(CxAuditTrailSupressions.SECTION_NAME);

			//var crawler = new DBCrawler(outFactory);

			//logicController.ExecuteCrawlOnTable(supressed.CxActivity_dbo_AuditTrail, CxAuditTrailConsts.CxActivity_dbo_AuditTrail);



			


		}
	}
}
