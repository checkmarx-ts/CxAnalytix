using System;
using CxAnalytix.Interfaces.Outputs;
using CxAnalytix.Interfaces.Audit;
using CxAnalytix.CxAuditTrails.DB;
using System.Data.SqlClient;
using System.Collections.Generic;
using Newtonsoft.Json;
using System.IO;
using System.Security.Cryptography.X509Certificates;

namespace CxAnalytix.CxAuditTrails
{
	public class DBCrawler : IAuditTrailCrawler
	{

		private DbAccess _db = new DbAccess();

		public DBCrawler ()
		{

		}

		private void OutputRecords (SqlDataReader reader, IOutput output, 
			Dictionary<String, Func<Object, Object> > customColumnConverters = null)
		{
			while (reader.Read () )
			{
				SortedDictionary<String, Object> rec = new SortedDictionary<string, object>();

				for (int x = 0; x < reader.FieldCount; x++)
				{
					var colName = reader.GetColumnSchema()[x].ColumnName;
					Object insertVal = customColumnConverters != null && customColumnConverters.ContainsKey (colName) ? 
						customColumnConverters [colName] (reader[x]) : reader[x];

					rec.Add(reader.GetColumnSchema()[x].ColumnName, insertVal);
				}

				output.write(rec);
			}
		}

		public void CxDB_accesscontrol_AuditTrail(DateTime sinceDate, IOutput output)
		{

			Func<Object, Object> detailsConverter = (val) =>
			{
				var serializer = JsonSerializer.Create();

				JsonTextReader jtr = new JsonTextReader(new StringReader(val as String));

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


	}
}
