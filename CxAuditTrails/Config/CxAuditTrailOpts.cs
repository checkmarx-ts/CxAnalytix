using System;
using System.Configuration;

namespace CxAuditTrails.Config
{
	public class CxAuditTrailOpts : ConfigurationSection
	{

		public static readonly String SECTION_NAME = "CxAuditTrailSupressions";

		[ConfigurationProperty(CxAuditTrailConsts.CxDB_accesscontrol_AuditTrail, IsRequired = false)]
		public bool Suppress_CxDB_accesscontrol_AuditTrail 
		{
			get => (bool)this[CxAuditTrailConsts.CxDB_accesscontrol_AuditTrail];
			set { this[CxAuditTrailConsts.CxDB_accesscontrol_AuditTrail] = value; }
		}

		[ConfigurationProperty(CxAuditTrailConsts.CxActivity_dbo_AuditTrail, IsRequired = false)]
		public bool Suppress_CxActivity_dbo_AuditTrail
		{
			get => (bool)this[CxAuditTrailConsts.CxActivity_dbo_AuditTrail];
			set { this[CxAuditTrailConsts.CxActivity_dbo_AuditTrail] = value; }
		}

		[ConfigurationProperty(CxAuditTrailConsts.CxActivity_dbo_Audit_DataRetention, IsRequired = false)]
		public bool Suppress_CxActivity_dbo_Audit_DataRetention
		{
			get => (bool)this[CxAuditTrailConsts.CxActivity_dbo_Audit_DataRetention];
			set { this[CxAuditTrailConsts.CxActivity_dbo_Audit_DataRetention] = value; }
		}

		[ConfigurationProperty(CxAuditTrailConsts.CxActivity_dbo_Audit_Logins, IsRequired = false)]
		public bool Suppress_CxActivity_dbo_Audit_Logins
		{
			get => (bool)this[CxAuditTrailConsts.CxActivity_dbo_Audit_Logins];
			set { this[CxAuditTrailConsts.CxActivity_dbo_Audit_Logins] = value; }
		}

		[ConfigurationProperty(CxAuditTrailConsts.CxActivity_dbo_Audit_Presets, IsRequired = false)]
		public bool Suppress_CxActivity_dbo_Audit_Presets
		{
			get => (bool)this[CxAuditTrailConsts.CxActivity_dbo_Audit_Presets];
			set { this[CxAuditTrailConsts.CxActivity_dbo_Audit_Presets] = value; }
		}

		[ConfigurationProperty(CxAuditTrailConsts.CxActivity_dbo_Audit_Projects, IsRequired = false)]
		public bool Suppress_CxActivity_dbo_Audit_Projects
		{
			get => (bool)this[CxAuditTrailConsts.CxActivity_dbo_Audit_Projects];
			set { this[CxAuditTrailConsts.CxActivity_dbo_Audit_Projects] = value; }
		}

		[ConfigurationProperty(CxAuditTrailConsts.CxActivity_dbo_Audit_Queries, IsRequired = false)]
		public bool Suppress_CxActivity_dbo_Audit_Queries
		{
			get => (bool)this[CxAuditTrailConsts.CxActivity_dbo_Audit_Queries];
			set { this[CxAuditTrailConsts.CxActivity_dbo_Audit_Queries] = value; }
		}

		[ConfigurationProperty(CxAuditTrailConsts.CxActivity_dbo_Audit_QueriesActions, IsRequired = false)]
		public bool Suppress_CxActivity_dbo_Audit_QueriesActions
		{
			get => (bool)this[CxAuditTrailConsts.CxActivity_dbo_Audit_QueriesActions];
			set { this[CxAuditTrailConsts.CxActivity_dbo_Audit_QueriesActions] = value; }
		}

		[ConfigurationProperty(CxAuditTrailConsts.CxActivity_dbo_Audit_Reports, IsRequired = false)]
		public bool Suppress_CxActivity_dbo_Audit_Reports
		{
			get => (bool)this[CxAuditTrailConsts.CxActivity_dbo_Audit_Reports];
			set { this[CxAuditTrailConsts.CxActivity_dbo_Audit_Reports] = value; }
		}

		[ConfigurationProperty(CxAuditTrailConsts.CxActivity_dbo_Audit_ScanRequests, IsRequired = false)]
		public bool Suppress_CxActivity_dbo_Audit_ScanRequests
		{
			get => (bool)this[CxAuditTrailConsts.CxActivity_dbo_Audit_ScanRequests];
			set { this[CxAuditTrailConsts.CxActivity_dbo_Audit_ScanRequests] = value; }
		}

		[ConfigurationProperty(CxAuditTrailConsts.CxActivity_dbo_Audit_Scans, IsRequired = false)]
		public bool Suppress_CxActivity_dbo_Audit_Scans
		{
			get => (bool)this[CxAuditTrailConsts.CxActivity_dbo_Audit_Scans];
			set { this[CxAuditTrailConsts.CxActivity_dbo_Audit_Scans] = value; }
		}

		[ConfigurationProperty(CxAuditTrailConsts.CxActivity_dbo_Audit_Users, IsRequired = false)]
		public bool Suppress_CxActivity_dbo_Audit_Users
		{
			get => (bool)this[CxAuditTrailConsts.CxActivity_dbo_Audit_Users];
			set { this[CxAuditTrailConsts.CxActivity_dbo_Audit_Users] = value; }
		}

	}
}
