using System;
using CxAnalytix.Interfaces.Outputs;
using CxAnalytix.Interfaces.Audit;
using CxAnalytix.CxAuditTrails.DB;
using System.Data.SqlClient;
using System.Collections.Generic;
using Newtonsoft.Json;
using System.IO;
using log4net;

namespace CxAnalytix.CxAuditTrails
{
	public class DBCrawler : IAuditTrailCrawler
	{

		private DbAccess _db = new DbAccess();
		private static readonly ILog _log = LogManager.GetLogger(typeof (DBCrawler));

		public DBCrawler ()
		{

		}

		public bool IsDisabled
		{
			get
			{
				return _db.IsDisabled;
			}
		}

		private void OutputRecords (SqlDataReader reader, IOutput output, 
			Dictionary<String, Func<Object, Object> > customColumnConverters = null)
		{
			int count = 0;

			while (reader.Read () )
			{
				SortedDictionary<String, Object> rec = new SortedDictionary<string, object>();

				for (int x = 0; x < reader.FieldCount; x++)
				{
					var colName = reader.GetColumnSchema()[x].ColumnName;

					var insertVal = reader[x];

					if (insertVal.GetType() == typeof(System.DBNull))
						continue;

					if (customColumnConverters != null && customColumnConverters.ContainsKey(colName))
						insertVal = customColumnConverters[colName](reader[x]);


					rec.Add(reader.GetColumnSchema()[x].ColumnName, insertVal);
				}

				output.write(rec);
				count++;
			}

			_log.Debug($"Wrote {count} audit records.");
		}

		public void CxDB_accesscontrol_AuditTrail(DateTime sinceDate, IOutput output)
		{

			Func<Object, Object> detailsConverter = (val) =>
			{
				var serializer = JsonSerializer.Create();

				using (JsonTextReader jtr = new JsonTextReader(new StringReader(val as String)))
					return serializer.Deserialize<SortedDictionary<String, Object>>(jtr);
			};

			using (var reader = _db.FetchRecords_CxDB_accesscontrol_AuditTrail(sinceDate))
				OutputRecords(reader, output, new Dictionary<string, Func<object, object>>
				{
					{ "Details", detailsConverter}
				});
		}

		public void CxActivity_dbo_AuditTrail(DateTime sinceDate, IOutput output)
		{
			using (var reader = _db.FetchRecords_CxActivity_dbo_AuditTrail(sinceDate))
				OutputRecords(reader, output);
		}

		public void CxActivity_dbo_Audit_DataRetention(DateTime sinceDate, IOutput output)
		{
			using (var reader = _db.FetchRecords_CxActivity_dbo_Audit_DataRetention(sinceDate))
				OutputRecords(reader, output);
		}

		public void CxActivity_dbo_Audit_Logins(DateTime sinceDate, IOutput output)
		{
			using (var reader = _db.FetchRecords_CxActivity_dbo_Audit_Logins(sinceDate))
				OutputRecords(reader, output);
		}

		public void CxActivity_dbo_Audit_Presets(DateTime sinceDate, IOutput output)
		{
			using (var reader = _db.FetchRecords_CxActivity_dbo_Audit_Presets(sinceDate))
				OutputRecords(reader, output);
		}

		public void CxActivity_dbo_Audit_Projects(DateTime sinceDate, IOutput output)
		{
			using (var reader = _db.FetchRecords_CxActivity_dbo_Audit_Projects(sinceDate))
				OutputRecords(reader, output);
		}

		public void CxActivity_dbo_Audit_Queries(DateTime sinceDate, IOutput output)
		{
			using (var reader = _db.FetchRecords_CxActivity_dbo_Audit_Queries(sinceDate))
				OutputRecords(reader, output);
		}

		public void CxActivity_dbo_Audit_QueriesActions(DateTime sinceDate, IOutput output)
		{
			using (var reader = _db.FetchRecords_CxActivity_dbo_Audit_QueriesActions(sinceDate))
				OutputRecords(reader, output);
		}

		public void CxActivity_dbo_Audit_Reports(DateTime sinceDate, IOutput output)
		{
			using (var reader = _db.FetchRecords_CxActivity_dbo_Audit_Reports(sinceDate))
				OutputRecords(reader, output);
		}

		public void CxActivity_dbo_Audit_ScanRequests(DateTime sinceDate, IOutput output)
		{
			using (var reader = _db.FetchRecords_CxActivity_dbo_Audit_ScanRequests(sinceDate))
				OutputRecords(reader, output);
		}

		public void CxActivity_dbo_Audit_Scans(DateTime sinceDate, IOutput output)
		{
			using (var reader = _db.FetchRecords_CxActivity_dbo_Audit_Scans(sinceDate))
				OutputRecords(reader, output);
		}

		public void CxActivity_dbo_Audit_Users(DateTime sinceDate, IOutput output)
		{
			using (var reader = _db.FetchRecords_CxActivity_dbo_Audit_Users(sinceDate))
				OutputRecords(reader, output);
		}
	}
}
