using CxAnalytix.Configuration.Contracts;
using CxAnalytix.Configuration.Utils;
using System;
using System.Collections.Generic;
using System.Composition;
using System.Configuration;
using System.Text;
using System.Xml;
using System.Xml.Serialization;

namespace CxAnalytix.Configuration.Impls
{
	[SecureConfigSection(SensitiveStringProp = "Password") ]
	[SecureConfigSection(SensitiveStringProp = "Token")]
	[Export(typeof(ICxCredentials))]
	internal sealed class CxCredentials : EnvAwareConfigurationSection, ICxCredentials
	{
		public CxCredentials()
		{
			Config.AutoInit(this);
		}

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
