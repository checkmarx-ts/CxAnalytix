using System;
using System.Configuration;
using CxAnalytix.Configuration;

namespace CxAuditTrails.Config
{
	[SecureConfigSection(SensitiveStringProp = "ConnectionString")]
	class CxAuditDBConnection : EnvAwareConfigurationSection
	{
		public static readonly String SECTION_NAME = "CxDBCredentials";

		[ConfigurationProperty("ConnectionString", IsRequired = true)]
		public String ConnectionString
		{
			get => (String)this["ConnectionString"];
			set { this["ConnectionString"] = value; }
		}

	}
}
