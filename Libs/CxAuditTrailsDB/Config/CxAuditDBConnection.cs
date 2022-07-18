using System;
using System.Configuration;
using CxAnalytix.Configuration.Utils;

namespace CxAnalytix.CxAuditTrails.DB.Config
{
	[SecureConfigSection("ConnectionString")]
	class CxAuditDBConnection : EnvAwareConfigurationSection
	{
		public CxAuditDBConnection() { }


		[ConfigurationProperty("ConnectionString", IsRequired = true)]
		public String ConnectionString
		{
			get => (String)this["ConnectionString"];
			set { this["ConnectionString"] = value; }
		}

	}
}
