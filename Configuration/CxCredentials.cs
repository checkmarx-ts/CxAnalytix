using System;
using System.Collections.Generic;
using System.Configuration;
using System.Text;
using System.Xml;
using System.Xml.Serialization;

namespace CxAnalytix.Configuration
{
	[SecureConfigSection(SensitiveStringProp = "Password") ]
	[SecureConfigSection(SensitiveStringProp = "Token")]
	public sealed class CxCredentials : EnvAwareConfigurationSection
	{
		internal CxCredentials()
		{

		}

		public static readonly String SECTION_NAME = "CxCredentials";

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

		[ConfigurationProperty("Token", IsRequired = false)]
		public String Token
		{
			get => (String)this["Token"];
			set { this["Token"] = value; }
		}

	}
}
