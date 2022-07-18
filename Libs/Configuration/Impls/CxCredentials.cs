using CxAnalytix.Configuration.Utils;
using System;
using System.Configuration;

namespace CxAnalytix.Configuration.Impls
{
	[SecureConfigSection("Password") ]
	public class CxCredentials : EnvAwareConfigurationSection
	{

		[ConfigurationProperty("Username", IsRequired = true)]
		public String Username
		{
			get => (String)this["Username"];
			set { this["Username"] = value; }
		}

		[ConfigurationProperty("Password", IsRequired = true)]
		public String Password
		{
			get => (String)this["Password"];
			set { this["Password"] = value; }
		}
	}
}
