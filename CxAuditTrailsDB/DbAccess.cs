using System;
using System.Collections.Generic;
using System.Text;
using System.Data.SqlClient;
using CxAnalytix.CxAuditTrails.DB.Config;
using log4net;

namespace CxAnalytix.CxAuditTrails.DB
{
	public class DbAccess : IDisposable
	{
		private SqlConnection _con;
		private static ILog _log = LogManager.GetLogger(typeof (DbAccess) );


		public DbAccess ()
		{
			var cfg = CxAnalytix.Configuration.Config.GetConfig<CxAuditDBConnection>(CxAuditDBConnection.SECTION_NAME);

			_con = new SqlConnection(cfg.ConnectionString);
		}


		private bool TableExists (String dbName, String schemaName, String tableName)
		{
			bool retVal = false;


			var cmd = _con.CreateCommand();

			cmd.CommandText = @"USE {db} 
				SELECT t.name as TableName, s.name as SchemaName
				FROM sys.tables t 
				JOIN sys.schemas s ON s.schema_id = t.schema_id 
				WHERE t.name = {table} AND s.name = {schema}";

			cmd.Parameters.AddWithValue("db", dbName);
			cmd.Parameters.AddWithValue("table", tableName);
			cmd.Parameters.AddWithValue("schema", schemaName);

			try
			{
				_con.Open();
				var reader = cmd.ExecuteReader();
				retVal = reader.HasRows;
			}
			catch (Exception ex)
			{
				_log.Error($"Error probing for {dbName}.{schemaName}.{tableName} existence.", ex);
			}
			finally
			{
				_con.Close();
			}


			return retVal;
		}


		public bool AccessControlAuditTrailExists ()
		{
			return TableExists("CxDB", "accesscontrol", "AuditTrail");
		}


		public void Dispose()
		{
			if (_con != null)
			{
				if (_con.State == System.Data.ConnectionState.Open)
					_con.Close();

				_con.Dispose();
			}
		}
	}
}
