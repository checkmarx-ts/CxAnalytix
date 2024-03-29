﻿using RabbitMQ.Client;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Text;

namespace CxAnalytix.Out.AMQPOutput.Config.Impls
{
	public class AmqpEndpointConfig : ConfigurationElement
	{
		[ConfigurationProperty("AmqpUri", IsRequired = true)]
		public String AmqpUri
		{
			get => (String)this["AmqpUri"];
			set => base["AmqpUri"] = value;
		}

		[ConfigurationProperty("SSLOptions", IsRequired = false)]
		public AmqpEndpointSSLConfig SSLOpts
		{
			get => (AmqpEndpointSSLConfig)this["SSLOptions"];
			set => base["SSLOptions"] = value;
		}

		internal SslOption SSL
		{
			get
			{
				if (!SSLOpts.ElementInformation.IsPresent)
					return new SslOption() { Enabled = false };
				
				var retVal = new SslOption()
				{
					Version = System.Security.Authentication.SslProtocols.None,
					CertPassphrase = SSLOpts.CertPassphrase,
					CertPath = SSLOpts.CertPath,
					AcceptablePolicyErrors =
					  ((SSLOpts.CertChainError) ? (System.Net.Security.SslPolicyErrors.RemoteCertificateChainErrors) : (0))
					  | ((SSLOpts.CertNameMistmatch) ? (System.Net.Security.SslPolicyErrors.RemoteCertificateNameMismatch) : (0)),
					Enabled = true
				};

				if (!String.IsNullOrEmpty(SSLOpts.ServerName))
					retVal.ServerName = SSLOpts.ServerName;

				return retVal;

		}
	}







}
}
