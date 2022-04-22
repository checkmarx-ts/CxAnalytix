using System;
using System.Collections.Generic;
using System.Text;

namespace CxAnalytix.Configuration.Contracts
{
	public interface ICxCredentials
	{
		String Username { get; set; }
		String Password { get; set; }
	}
}
