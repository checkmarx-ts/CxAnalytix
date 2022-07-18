using CxAnalytix.Configuration.Utils;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CxAnalytix.Configuration.Impls
{

	[SecureConfigSection("Password")]
	public class CxMultiTenantCredentials : CxCredentials
    {
		[ConfigurationProperty("Tenant", IsRequired = true)]
		public String Tenant
		{
			get => (String)this["Tenant"];
			set { this["Tenant"] = value; }
		}

	}
}
