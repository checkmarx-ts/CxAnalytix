using CxAnalytix.Configuration.Contracts;
using CxAnalytix.Configuration.Utils;
using System;
using System.Composition;
using System.Configuration;

namespace CxAnalytix.Configuration.Impls
{
	[Export(typeof(ICxCredentials))]
	[SecureConfigSection("Password") ]
	public sealed class CxCredentials : EnvAwareConfigurationSection, ICxCredentials
	{
		public CxCredentials() { }

		[ImportingConstructor]
		public CxCredentials(IConfigSectionResolver resolver) : base(resolver) { }


		[ConfigurationProperty("Username", IsRequired = false)]
		public String Username
		{
			get => (String)Instance<CxCredentials>()["Username"];
			set { Instance<CxCredentials>()["Username"] = value; }
		}

		[ConfigurationProperty("Password", IsRequired = false)]
		public String Password
		{
			get => (String)Instance<CxCredentials>()["Password"];
			set { Instance<CxCredentials>()["Password"] = value; }
		}
	}
}
