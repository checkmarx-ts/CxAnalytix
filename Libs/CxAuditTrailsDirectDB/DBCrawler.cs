using System;
using CxAnalytix.Interfaces.Outputs;
using CxAnalytix.Interfaces.Audit;
using CxAnalytix.CxAuditTrails.DB;
using System.Collections.Generic;
using Newtonsoft.Json;
using System.IO;
using log4net;
using CxAnalytix.Extensions;
using Microsoft.Data.SqlClient;

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

		private void OutputRecords (SqlDataReader reader, IOutputTransaction trx, IRecordRef record, 
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

				trx.write(record, rec);
				count++;
			}

			_log.Trace($"Wrote {count} audit records.");
		}

		public void CxDB_accesscontrol_AuditTrail(DateTime sinceDate, IOutputTransaction trx, IRecordRef record)
		{

			_log.Debug("BEGIN: CxDB_accesscontrol_AuditTrail");
			Func<Object, Object> detailsConverter = (val) =>
			{
				var serializer = JsonSerializer.Create();

				using (var sr = new StringReader(val as String))
				using (JsonTextReader jtr = new JsonTextReader(sr))
					return serializer.Deserialize<SortedDictionary<String, Object>>(jtr);
			};

			using (var reader = _db.FetchRecords_CxDB_accesscontrol_AuditTrail(sinceDate))
				OutputRecords(reader.DataReader, trx, record, new Dictionary<string, Func<object, object>>
				{
					{ "Details", detailsConverter}
				});
        
			_log.Debug("END: CxDB_accesscontrol_AuditTrail");
        }

        public void CxActivity_dbo_AuditTrail(DateTime sinceDate, IOutputTransaction trx, IRecordRef record)
		{
            _log.Debug("BEGIN: CxActivity_dbo_AuditTrail");
            using (var reader = _db.FetchRecords_CxActivity_dbo_AuditTrail(sinceDate))
				OutputRecords(reader.DataReader, trx, record);
            _log.Debug("END: CxActivity_dbo_AuditTrail");
        }

        public void CxActivity_dbo_Audit_DataRetention(DateTime sinceDate, IOutputTransaction trx, IRecordRef record)
		{
            _log.Debug("BEGIN: CxActivity_dbo_Audit_DataRetention");
            using (var reader = _db.FetchRecords_CxActivity_dbo_Audit_DataRetention(sinceDate))
				OutputRecords(reader.DataReader, trx, record);
            _log.Debug("END: CxActivity_dbo_Audit_DataRetention");
        }

        public void CxActivity_dbo_Audit_Logins(DateTime sinceDate, IOutputTransaction trx, IRecordRef record)
		{
            _log.Debug("BEGIN: CxActivity_dbo_Audit_Logins");
            using (var reader = _db.FetchRecords_CxActivity_dbo_Audit_Logins(sinceDate))
				OutputRecords(reader.DataReader, trx, record);
            _log.Debug("END: CxActivity_dbo_Audit_Logins");
        }

        public void CxActivity_dbo_Audit_Presets(DateTime sinceDate, IOutputTransaction trx, IRecordRef record)
		{
            _log.Debug("BEGIN: CxActivity_dbo_Audit_Presets");
            using (var reader = _db.FetchRecords_CxActivity_dbo_Audit_Presets(sinceDate))
				OutputRecords(reader.DataReader, trx, record);
            _log.Debug("END: CxActivity_dbo_Audit_Presets");
        }

        public void CxActivity_dbo_Audit_Projects(DateTime sinceDate, IOutputTransaction trx, IRecordRef record)
		{
            _log.Debug("BEGIN: CxActivity_dbo_Audit_Projects");
            using (var reader = _db.FetchRecords_CxActivity_dbo_Audit_Projects(sinceDate))
				OutputRecords(reader.DataReader, trx, record);
            _log.Debug("END: CxActivity_dbo_Audit_Projects");
        }

        public void CxActivity_dbo_Audit_Queries(DateTime sinceDate, IOutputTransaction trx, IRecordRef record)
		{
            _log.Debug("BEGIN: CxActivity_dbo_Audit_Queries");
            using (var reader = _db.FetchRecords_CxActivity_dbo_Audit_Queries(sinceDate))
				OutputRecords(reader.DataReader, trx, record);
            _log.Debug("END: CxActivity_dbo_Audit_Queries");
        }

        public void CxActivity_dbo_Audit_QueriesActions(DateTime sinceDate, IOutputTransaction trx, IRecordRef record)
		{
            _log.Debug("BEGIN: CxActivity_dbo_Audit_QueriesActions");
            using (var reader = _db.FetchRecords_CxActivity_dbo_Audit_QueriesActions(sinceDate))
				OutputRecords(reader.DataReader, trx, record);
            _log.Debug("END: CxActivity_dbo_Audit_QueriesActions");
        }

        public void CxActivity_dbo_Audit_Reports(DateTime sinceDate, IOutputTransaction trx, IRecordRef record)
		{
            _log.Debug("BEGIN: CxActivity_dbo_Audit_Reports");
            using (var reader = _db.FetchRecords_CxActivity_dbo_Audit_Reports(sinceDate))
				OutputRecords(reader.DataReader, trx, record);
            _log.Debug("END: CxActivity_dbo_Audit_Reports");
        }

        public void CxActivity_dbo_Audit_ScanRequests(DateTime sinceDate, IOutputTransaction trx, IRecordRef record)
		{
            _log.Debug("BEGIN: CxActivity_dbo_Audit_ScanRequests");
            using (var reader = _db.FetchRecords_CxActivity_dbo_Audit_ScanRequests(sinceDate))
				OutputRecords(reader.DataReader, trx, record);
            _log.Debug("END: CxActivity_dbo_Audit_ScanRequests");
        }

        public void CxActivity_dbo_Audit_Scans(DateTime sinceDate, IOutputTransaction trx, IRecordRef record)
		{
            _log.Debug("BEGIN: CxActivity_dbo_Audit_Scans");
            using (var reader = _db.FetchRecords_CxActivity_dbo_Audit_Scans(sinceDate))
				OutputRecords(reader.DataReader, trx, record);
            _log.Debug("END: CxActivity_dbo_Audit_Scans");
        }

        public void CxActivity_dbo_Audit_Users(DateTime sinceDate, IOutputTransaction trx, IRecordRef record)
		{
            _log.Debug("BEGIN: CxActivity_dbo_Audit_Users");
            using (var reader = _db.FetchRecords_CxActivity_dbo_Audit_Users(sinceDate))
				OutputRecords(reader.DataReader, trx, record);
            _log.Debug("END: CxActivity_dbo_Audit_Users");
        }
    }
}
