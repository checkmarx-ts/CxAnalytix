using System;
using System.Collections.Generic;
using System.Configuration;
using System.Security.Authentication;
using System.Text;

namespace CxAnalytix.Out.AMQPOutput.Config
{
	public class AmqpEndpointSSLConfig : ConfigurationElement
	{

		[ConfigurationProperty("ServerName", IsRequired = false, DefaultValue = null)]
		public String ServerName
		{
			get => (String)base["ServerName"];
			set => base["ServerName"] = value;
		}

		[ConfigurationProperty("ClientCertPath", IsRequired = false, DefaultValue = null)]
		public String CertPath
		{
			get => (String)base["ClientCertPath"];
			set => base["ClientCertPath"] = value;
		}

		[ConfigurationProperty("ClientCertPassphrase", IsRequired = false, DefaultValue = null)]
		public String CertPassphrase
		{
			get => (String)base["ClientCertPassphrase"];
			set => base["ClientCertPassphrase"] = value;
		}

		[ConfigurationProperty("AllowCertNameMismatch", IsRequired = false, DefaultValue = false)]
		public bool CertNameMistmatch
		{
			get => (bool)base["AllowCertNameMismatch"];
			set => base["AllowCertNameMismatch"] = value;
		}

		[ConfigurationProperty("AllowCertificateChainErrors", IsRequired = false, DefaultValue = false)]
		public bool CertChainError
		{
			get => (bool)base["AllowCertificateChainErrors"];
			set => base["AllowCertificateChainErrors"] = value;
		}

	}
}
