using System;
using System.Configuration;

namespace CxAnalytix.AuditTrails.Crawler.Config
{
	public class CxAuditTrailOpts<T> : ConfigurationSection
	{
		public CxAuditTrailOpts() {}

		private Func<String, T> _default = (x) => default(T);

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

		protected override void InitializeDefault()
		{
			base.InitializeDefault();

			var constsInst = new CxAuditTrailTableNameConsts();

			foreach (var prop in typeof(CxAuditTrailOpts<T>).GetProperties())
			{
				var field = constsInst.GetType().GetField(prop.Name);
				if (field == null)
					continue;

				var defaultKeyVal = field.GetValue(constsInst) as String;


				GetType ().InvokeMember(prop.Name, System.Reflection.BindingFlags.SetProperty, 
					null, this, new Object[] { _default(defaultKeyVal) } );

			}
		}

		[ConfigurationProperty(CxAuditTrailTableNameConsts.CxDB_accesscontrol_AuditTrail, IsRequired = false)]
		public T CxDB_accesscontrol_AuditTrail 
		{
			get => GetPropertyValue (CxAuditTrailTableNameConsts.CxDB_accesscontrol_AuditTrail);
			set => SetPropertyValue (CxAuditTrailTableNameConsts.CxDB_accesscontrol_AuditTrail, value);
		}

		[ConfigurationProperty(CxAuditTrailTableNameConsts.CxActivity_dbo_AuditTrail, IsRequired = false)]
		public T CxActivity_dbo_AuditTrail
		{
			get => GetPropertyValue(CxAuditTrailTableNameConsts.CxActivity_dbo_AuditTrail);
			set => SetPropertyValue(CxAuditTrailTableNameConsts.CxActivity_dbo_AuditTrail, value);
		}

		[ConfigurationProperty(CxAuditTrailTableNameConsts.CxActivity_dbo_Audit_DataRetention, IsRequired = false)]
		public T CxActivity_dbo_Audit_DataRetention
		{
			get => GetPropertyValue(CxAuditTrailTableNameConsts.CxActivity_dbo_Audit_DataRetention);
			set => SetPropertyValue(CxAuditTrailTableNameConsts.CxActivity_dbo_Audit_DataRetention, value);
		}

		[ConfigurationProperty(CxAuditTrailTableNameConsts.CxActivity_dbo_Audit_Logins, IsRequired = false)]
		public T CxActivity_dbo_Audit_Logins
		{
			get => GetPropertyValue(CxAuditTrailTableNameConsts.CxActivity_dbo_Audit_Logins);
			set => SetPropertyValue(CxAuditTrailTableNameConsts.CxActivity_dbo_Audit_Logins, value); 
		}

		[ConfigurationProperty(CxAuditTrailTableNameConsts.CxActivity_dbo_Audit_Presets, IsRequired = false)]
		public T CxActivity_dbo_Audit_Presets
		{
			get => GetPropertyValue(CxAuditTrailTableNameConsts.CxActivity_dbo_Audit_Presets);
			set => SetPropertyValue(CxAuditTrailTableNameConsts.CxActivity_dbo_Audit_Presets, value);
		}

		[ConfigurationProperty(CxAuditTrailTableNameConsts.CxActivity_dbo_Audit_Projects, IsRequired = false)]
		public T CxActivity_dbo_Audit_Projects
		{
			get => GetPropertyValue(CxAuditTrailTableNameConsts.CxActivity_dbo_Audit_Projects);
			set => SetPropertyValue(CxAuditTrailTableNameConsts.CxActivity_dbo_Audit_Projects, value);
		}

		[ConfigurationProperty(CxAuditTrailTableNameConsts.CxActivity_dbo_Audit_Queries, IsRequired = false)]
		public T CxActivity_dbo_Audit_Queries
		{
			get => GetPropertyValue(CxAuditTrailTableNameConsts.CxActivity_dbo_Audit_Queries);
			set => SetPropertyValue(CxAuditTrailTableNameConsts.CxActivity_dbo_Audit_Queries, value);
		}

		[ConfigurationProperty(CxAuditTrailTableNameConsts.CxActivity_dbo_Audit_QueriesActions, IsRequired = false)]
		public T CxActivity_dbo_Audit_QueriesActions
		{
			get => GetPropertyValue(CxAuditTrailTableNameConsts.CxActivity_dbo_Audit_QueriesActions);
			set => SetPropertyValue(CxAuditTrailTableNameConsts.CxActivity_dbo_Audit_QueriesActions, value);
		}

		[ConfigurationProperty(CxAuditTrailTableNameConsts.CxActivity_dbo_Audit_Reports, IsRequired = false)]
		public T CxActivity_dbo_Audit_Reports
		{
			get => GetPropertyValue(CxAuditTrailTableNameConsts.CxActivity_dbo_Audit_Reports);
			set => SetPropertyValue(CxAuditTrailTableNameConsts.CxActivity_dbo_Audit_Reports, value);
		}

		[ConfigurationProperty(CxAuditTrailTableNameConsts.CxActivity_dbo_Audit_ScanRequests, IsRequired = false)]
		public T CxActivity_dbo_Audit_ScanRequests
		{
			get => GetPropertyValue(CxAuditTrailTableNameConsts.CxActivity_dbo_Audit_ScanRequests);
			set => SetPropertyValue(CxAuditTrailTableNameConsts.CxActivity_dbo_Audit_ScanRequests, value);
		}

		[ConfigurationProperty(CxAuditTrailTableNameConsts.CxActivity_dbo_Audit_Scans, IsRequired = false)]
		public T CxActivity_dbo_Audit_Scans
		{
			get => GetPropertyValue(CxAuditTrailTableNameConsts.CxActivity_dbo_Audit_Scans);
			set => SetPropertyValue(CxAuditTrailTableNameConsts.CxActivity_dbo_Audit_Scans, value);
		}

		[ConfigurationProperty(CxAuditTrailTableNameConsts.CxActivity_dbo_Audit_Users, IsRequired = false)]
		public T CxActivity_dbo_Audit_Users
		{
			get => GetPropertyValue(CxAuditTrailTableNameConsts.CxActivity_dbo_Audit_Users);
			set => SetPropertyValue(CxAuditTrailTableNameConsts.CxActivity_dbo_Audit_Users, value);
		}

	}
}
