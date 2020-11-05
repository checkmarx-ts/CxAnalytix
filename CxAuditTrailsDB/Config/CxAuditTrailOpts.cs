using System;
using System.Configuration;

namespace CxAnalytix.CxAuditTrails.Config
{
	public class CxAuditTrailOpts<T> : ConfigurationSection
	{

		private Func<String, T> _default;

		public CxAuditTrailOpts(Func<String, T> createDefault)
		{
			_default = createDefault;
		}

		private T GetPropertyValue (String key)
		{
			if (!this.Properties.Contains(key))
				return _default(key);

			return (T)this[key];
		}

		private void SetPropertyValue (String key, T val)
		{
			this[key] = val;
		}

				
		[ConfigurationProperty(CxAuditTrailConsts.CxDB_accesscontrol_AuditTrail, IsRequired = false)]
		public T CxDB_accesscontrol_AuditTrail 
		{
			get => GetPropertyValue (CxAuditTrailConsts.CxDB_accesscontrol_AuditTrail);
			set => SetPropertyValue (CxAuditTrailConsts.CxDB_accesscontrol_AuditTrail, value);
		}

		[ConfigurationProperty(CxAuditTrailConsts.CxActivity_dbo_AuditTrail, IsRequired = false)]
		public T CxActivity_dbo_AuditTrail
		{
			get => GetPropertyValue(CxAuditTrailConsts.CxActivity_dbo_AuditTrail);
			set => SetPropertyValue(CxAuditTrailConsts.CxActivity_dbo_AuditTrail, value);
		}

		[ConfigurationProperty(CxAuditTrailConsts.CxActivity_dbo_Audit_DataRetention, IsRequired = false)]
		public T CxActivity_dbo_Audit_DataRetention
		{
			get => GetPropertyValue(CxAuditTrailConsts.CxActivity_dbo_Audit_DataRetention);
			set => SetPropertyValue(CxAuditTrailConsts.CxActivity_dbo_Audit_DataRetention, value);
		}

		[ConfigurationProperty(CxAuditTrailConsts.CxActivity_dbo_Audit_Logins, IsRequired = false)]
		public T CxActivity_dbo_Audit_Logins
		{
			get => GetPropertyValue(CxAuditTrailConsts.CxActivity_dbo_Audit_Logins);
			set => SetPropertyValue(CxAuditTrailConsts.CxActivity_dbo_Audit_Logins, value); 
		}

		[ConfigurationProperty(CxAuditTrailConsts.CxActivity_dbo_Audit_Presets, IsRequired = false)]
		public T CxActivity_dbo_Audit_Presets
		{
			get => GetPropertyValue(CxAuditTrailConsts.CxActivity_dbo_Audit_Presets);
			set => SetPropertyValue(CxAuditTrailConsts.CxActivity_dbo_Audit_Presets, value);
		}

		[ConfigurationProperty(CxAuditTrailConsts.CxActivity_dbo_Audit_Projects, IsRequired = false)]
		public T CxActivity_dbo_Audit_Projects
		{
			get => GetPropertyValue(CxAuditTrailConsts.CxActivity_dbo_Audit_Projects);
			set => SetPropertyValue(CxAuditTrailConsts.CxActivity_dbo_Audit_Projects, value);
		}

		[ConfigurationProperty(CxAuditTrailConsts.CxActivity_dbo_Audit_Queries, IsRequired = false)]
		public T CxActivity_dbo_Audit_Queries
		{
			get => GetPropertyValue(CxAuditTrailConsts.CxActivity_dbo_Audit_Queries);
			set => SetPropertyValue(CxAuditTrailConsts.CxActivity_dbo_Audit_Queries, value);
		}

		[ConfigurationProperty(CxAuditTrailConsts.CxActivity_dbo_Audit_QueriesActions, IsRequired = false)]
		public T CxActivity_dbo_Audit_QueriesActions
		{
			get => GetPropertyValue(CxAuditTrailConsts.CxActivity_dbo_Audit_QueriesActions);
			set => SetPropertyValue(CxAuditTrailConsts.CxActivity_dbo_Audit_QueriesActions, value);
		}

		[ConfigurationProperty(CxAuditTrailConsts.CxActivity_dbo_Audit_Reports, IsRequired = false)]
		public T CxActivity_dbo_Audit_Reports
		{
			get => GetPropertyValue(CxAuditTrailConsts.CxActivity_dbo_Audit_Reports);
			set => SetPropertyValue(CxAuditTrailConsts.CxActivity_dbo_Audit_Reports, value);
		}

		[ConfigurationProperty(CxAuditTrailConsts.CxActivity_dbo_Audit_ScanRequests, IsRequired = false)]
		public T CxActivity_dbo_Audit_ScanRequests
		{
			get => GetPropertyValue(CxAuditTrailConsts.CxActivity_dbo_Audit_ScanRequests);
			set => SetPropertyValue(CxAuditTrailConsts.CxActivity_dbo_Audit_ScanRequests, value);
		}

		[ConfigurationProperty(CxAuditTrailConsts.CxActivity_dbo_Audit_Scans, IsRequired = false)]
		public T CxActivity_dbo_Audit_Scans
		{
			get => GetPropertyValue(CxAuditTrailConsts.CxActivity_dbo_Audit_Scans);
			set => SetPropertyValue(CxAuditTrailConsts.CxActivity_dbo_Audit_Scans, value);
		}

		[ConfigurationProperty(CxAuditTrailConsts.CxActivity_dbo_Audit_Users, IsRequired = false)]
		public T CxActivity_dbo_Audit_Users
		{
			get => GetPropertyValue(CxAuditTrailConsts.CxActivity_dbo_Audit_Users);
			set => SetPropertyValue(CxAuditTrailConsts.CxActivity_dbo_Audit_Users, value);
		}

	}
}
