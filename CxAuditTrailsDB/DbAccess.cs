using System;
using System.Collections.Generic;
using System.Text;
using System.Data.SqlClient;
using CxAnalytix.CxAuditTrails.DB.Config;
using log4net;

namespace CxAnalytix.CxAuditTrails.DB
{
	public class DbAccess
	{
		private static ILog _log = LogManager.GetLogger(typeof(DbAccess));
		private String _conStr;


		public DbAccess()
		{
			var cfg = CxAnalytix.Configuration.Config.GetConfig<CxAuditDBConnection>(CxAuditDBConnection.SECTION_NAME);
			_conStr = cfg.ConnectionString;

		}


		private bool TableExists(String dbName, String schemaName, String tableName)
		{
			bool retVal = false;

			using (SqlConnection con = new SqlConnection(_conStr))
			{
				var cmd = con.CreateCommand();

				cmd.CommandText = @"SELECT t.name as TableName, s.name as SchemaName
				FROM sys.tables t 
				JOIN sys.schemas s ON s.schema_id = t.schema_id 
				WHERE t.name = @TABLE AND s.name = @SCHEMA";

				cmd.Parameters.AddWithValue("@TABLE", tableName);
				cmd.Parameters.AddWithValue("@SCHEMA", schemaName);

				try
				{
					con.Open();
					con.ChangeDatabase(dbName);
					var reader = cmd.ExecuteReader(System.Data.CommandBehavior.SingleRow);
					retVal = reader.HasRows;
				}
				catch (Exception ex)
				{
					_log.Error($"Error probing for {dbName}.{schemaName}.{tableName} existence.", ex);
				}
			}

			return retVal;
		}

		public SqlDataReader FetchRecords_CxDB_accesscontrol_AuditTrail(DateTime since)
		{
			if (!TableExists("CxDB", "accesscontrol", "AuditTrail"))
				throw new TableDoesNotExistException("CxDB", "accesscontrol", "AuditTrail");

			SqlConnection con = new SqlConnection(_conStr);

			con.Open();
			con.ChangeDatabase("CxDB");

			var cmd = con.CreateCommand();

			cmd.CommandText = @"SELECT * FROM [CxDB].[accesscontrol].[AuditTrail] WHERE Timestamp > @SINCE";

			cmd.Parameters.AddWithValue("@SINCE", since);

			return cmd.ExecuteReader(System.Data.CommandBehavior.CloseConnection);
		}

		public SqlDataReader FetchRecords_CxActivity_dbo_AuditTrail(DateTime since)
		{
			if (!TableExists("CxActivity", "dbo", "AuditTrail"))
				throw new TableDoesNotExistException("CxDB", "accesscontrol", "AuditTrail");

			SqlConnection con = new SqlConnection(_conStr);

			con.Open();
			con.ChangeDatabase("CxActivity");

			var cmd = con.CreateCommand();

			cmd.CommandText = @"SELECT 
				ActTyp.Name as [Action]
				,EntTyp.Name as [EntityType]
				,AudTrl.EntityId
				,AudTrl.StartTime
				,AudTrl.EndTime
				,AudTrl.Origin
				,CxUsers.UserName
				,AudTrl.Success
				,AudTrl.Remarks
				FROM CxActivity.dbo.AuditTrail as AudTrl
				JOIN CxActivity.dbo.ActionType as ActTyp ON AudTrl.Action = ActTyp.Id
				JOIN CxActivity.dbo.EntityType as EntTyp ON AudTrl.EntityType = EntTyp.Id
				JOIN CxDB.dbo.Users as CxUsers ON AudTrl.UserId = CxUsers.ID
				WHERE AudTrl.EndTime > @SINCE";

			cmd.Parameters.AddWithValue("@SINCE", since);

			return cmd.ExecuteReader(System.Data.CommandBehavior.CloseConnection);


		}



	}
}
