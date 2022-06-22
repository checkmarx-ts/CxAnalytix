using System;
using System.Composition;
using System.Configuration;
using CxAnalytix.Configuration.Contracts;
using CxAnalytix.Configuration.Utils;
using CxAnalytix.CxAuditTrails.DB.Contracts;

namespace CxAnalytix.CxAuditTrails.DB.Config
{
	[Export(typeof(ICxAuditDBConnection))]
	[SecureConfigSection("ConnectionString")]
	class CxAuditDBConnection : EnvAwareConfigurationSection, ICxAuditDBConnection
	{
		public CxAuditDBConnection() { }

		public CxAuditDBConnection(IConfigSectionResolver resolver) : base(resolver) { }

		[ConfigurationProperty("ConnectionString", IsRequired = true)]
		public String ConnectionString
		{
			get => (String)Instance<CxAuditDBConnection>()["ConnectionString"];
			set { Instance<CxAuditDBConnection>()["ConnectionString"] = value; }
		}

	}
}
