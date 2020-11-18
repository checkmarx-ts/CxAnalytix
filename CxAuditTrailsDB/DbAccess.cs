using System;
using System.Data.SqlClient;
using CxAnalytix.CxAuditTrails.DB.Config;
using log4net;

namespace CxAnalytix.CxAuditTrails.DB
{
	public class DbAccess
	{
		private static ILog _log = LogManager.GetLogger(typeof(DbAccess));
		private String _conStr;
		private CxAuditDBConnection _cfg;


		public DbAccess()
		{
			_cfg = CxAnalytix.Configuration.Config.GetConfig<CxAuditDBConnection>(CxAuditDBConnection.SECTION_NAME);
			_conStr = _cfg.ConnectionString;

		}

		public bool IsDisabled
		{
			get
			{
				return !_cfg.ElementInformation.IsPresent;
			}
		}

		private bool TableExists(String dbName, String schemaName, String tableName)
		{
			bool retVal = false;

			using (SqlConnection con = new SqlConnection(_conStr))
			{
				var cmd = con.CreateCommand();

				cmd.CommandText = @"SELECT 
					t.name as TableName, s.name as SchemaName
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
					throw ex;
				}
			}

			return retVal;
		}

		internal SqlDataReader FetchRecords (String db, String schema, String table, String cmdText, DateTime since)
		{
			if (IsDisabled)
				throw new InvalidOperationException("Database access parameters are not defined.");

			if (!TableExists(db, schema, table))
				throw new TableDoesNotExistException(db, schema, table);

			SqlConnection con = new SqlConnection(_conStr);

			con.Open();
			con.ChangeDatabase(db);

			var cmd = con.CreateCommand();

			cmd.CommandText = cmdText;

			cmd.Parameters.AddWithValue("@SINCE", since);

			return cmd.ExecuteReader(System.Data.CommandBehavior.CloseConnection);
		}

		public SqlDataReader FetchRecords_CxDB_accesscontrol_AuditTrail(DateTime since)
		{
			String query  = @"SELECT 
				* 
				FROM [CxDB].[accesscontrol].[AuditTrail] 
				WHERE Timestamp > @SINCE";

			return FetchRecords("CxDB", "accesscontrol", "AuditTrail", query, since);
		}

		public SqlDataReader FetchRecords_CxActivity_dbo_AuditTrail(DateTime since)
		{
			String query = @"SELECT 
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

			return FetchRecords("CxActivity", "dbo", "AuditTrail", query, since);
		}

		public SqlDataReader FetchRecords_CxActivity_dbo_Audit_DataRetention(DateTime since)
		{
			String query = @"SELECT 
				[Id]
     			,[Timestamp] as TimeStamp
     			,[DataRetentionRequestId]
     			,[InitiatorName]
     			,[DeletedScanId]
 				FROM [CxActivity].[dbo].[Audit_DataRetention]				
				WHERE Timestamp > @SINCE";


			return FetchRecords("CxActivity", "dbo", "Audit_DataRetention", query, since);
		}

		public SqlDataReader FetchRecords_CxActivity_dbo_Audit_Logins(DateTime since)
		{
			String query = @"SELECT 
				[Id]
     			,[OwnerId]
     			,[OwnerName]
     			,[TimeStamp]
     			,[Event]
     			,[Client]
     			,[IsSuccessfull]
     			,[ErrorMessage]
 				FROM [CxActivity].[dbo].[Audit_Logins]
				WHERE TimeStamp > @SINCE";

			return FetchRecords("CxActivity", "dbo", "Audit_Logins", query, since);
		}

		public SqlDataReader FetchRecords_CxActivity_dbo_Audit_Presets(DateTime since)
		{
			String query = @"SELECT 
				[Id]
     			,[OwnerId]
     			,[OwnerName]
     			,[TimeStamp]
     			,[Event]
     			,[PresetId]
     			,[PresetName]
     			,[OwnerType]
     			,[IsSuccessfull]
 				FROM [CxActivity].[dbo].[Audit_Presets]
				WHERE TimeStamp > @SINCE";

			return FetchRecords("CxActivity", "dbo", "Audit_Presets", query, since);
		}

		public SqlDataReader FetchRecords_CxActivity_dbo_Audit_Projects(DateTime since)
		{
			String query = @"SELECT 
				[Id]
     			,[OwnerId]
     			,[OwnerName]
     			,[TimeStamp]
     			,[Event]
     			,[ProjectId]
     			,[ProjectName]
     			,[Client]
 				FROM [CxActivity].[dbo].[Audit_Projects]				
				WHERE TimeStamp > @SINCE";

			return FetchRecords("CxActivity", "dbo", "Audit_Projects", query, since);
		}

		public SqlDataReader FetchRecords_CxActivity_dbo_Audit_Queries(DateTime since)
		{
			String query = @"SELECT 
				[Id]
     			,[OwnerId]
     			,[OwnerName]
     			,[Event]
     			,[TimeStamp]
     			,[QueryId]
     			,[PackageId]
     			,[Name]
     			,[Source]
     			,[DraftSource]
     			,[Cwe]
     			,[Comments]
     			,[Severity]
     			,[isExecutable]
     			,[isEncrypted]
     			,[is_deprecated]
     			,[IsCheckOut]
     			,[UpdateTime]
     			,[CurrentUserName]
     			,[IsCompiled]
     			,[CxDescriptionID]
     			,[IsSuccessful]
     			,[Version]
     			,[OwnerType]
     			,[EngineMetadata]
 				FROM [CxActivity].[dbo].[Audit_Queries]
				WHERE TimeStamp > @SINCE";

			return FetchRecords("CxActivity", "dbo", "Audit_Queries", query, since);
		}

		public SqlDataReader FetchRecords_CxActivity_dbo_Audit_QueriesActions(DateTime since)
		{
			String query = @"SELECT 
				[Id]
     			,[OwnerId]
     			,[OwnerName]
     			,[OwnerType]
     			,[TimeStamp]
     			,[Event]
     			,[Comment]
     			,[IsSuccessfull]
 				FROM [CxActivity].[dbo].[Audit_QueriesActions]
				WHERE TimeStamp > @SINCE";

			return FetchRecords("CxActivity", "dbo", "Audit_QueriesActions", query, since);
		}

		public SqlDataReader FetchRecords_CxActivity_dbo_Audit_Reports(DateTime since)
		{
			String query = @"SELECT 
 				[ID] as Id
     			,[OwnerId]
     			,[OwnerName]
     			,[TimeStamp]
     			,[ScanID]
     			,[ReportTypeID]
     			,[ReportType]
 				FROM [CxActivity].[dbo].[Audit_Reports]
				WHERE TimeStamp > @SINCE";

			return FetchRecords("CxActivity", "dbo", "Audit_Reports", query, since);
		}

		public SqlDataReader FetchRecords_CxActivity_dbo_Audit_ScanRequests(DateTime since)
		{
			String query = @"SELECT 
				[Id]
     			,[EventInitiatorUserId]
     			,[EventInitiatorUserName]
     			,[ScanOwnerName]
     			,[TimeStamp]
     			,[Event]
     			,[ScanRequestId]
     			,[ScanID]
     			,[ProjectId]
     			,[ProjectName]
     			,[IsSuccessfull]
 				FROM [CxActivity].[dbo].[Audit_ScanRequests]
				WHERE TimeStamp > @SINCE";

			return FetchRecords("CxActivity", "dbo", "Audit_ScanRequests", query, since);
		}

		public SqlDataReader FetchRecords_CxActivity_dbo_Audit_Scans(DateTime since)
		{
			String query = @"SELECT 
				[Id]
     			,[OwnerId]
     			,[OwnerName]
     			,[Event]
     			,[TimeStamp]
     			,[ProjectId]
     			,[ProjectName]
     			,[Client]
     			,[ScanID]
     			,[UpdateType]
     			,[IsSuccessfull]
 				FROM [CxActivity].[dbo].[Audit_Scans]				
				WHERE TimeStamp > @SINCE";

			return FetchRecords("CxActivity", "dbo", "Audit_Scans", query, since);
		}

		public SqlDataReader FetchRecords_CxActivity_dbo_Audit_Users(DateTime since)
		{
			String query = @"SELECT 
				[Id]
     			,[OwnerId]
     			,[OwnerName]
     			,[TimeStamp]
     			,[Event]
     			,[UserId]
     			,[UserName]
     			,[IsSuccessfull]
     			,[Comment]
 				FROM [CxActivity].[dbo].[Audit_Users]
				WHERE TimeStamp > @SINCE";

			return FetchRecords("CxActivity", "dbo", "Audit_Users", query, since);
		}

	}
}
