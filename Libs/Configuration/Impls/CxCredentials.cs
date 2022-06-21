using CxAnalytix.Configuration.Utils;
using System;
using System.Configuration;

namespace CxAnalytix.Configuration.Impls
{
	[SecureConfigSection(SensitiveStringProp = "Password") ]
	internal sealed class CxCredentials : EnvAwareConfigurationSection
	{

        [ConfigurationProperty("Username", IsRequired = false)]
		public String Username
		{
			get => (String)this["Username"];
			set { this["Username"] = value; }
		}

		[ConfigurationProperty("Password", IsRequired = false)]
		public String Password
		{
			get => (String)this["Password"];
			set { this["Password"] = value; }
		}
	}
}
