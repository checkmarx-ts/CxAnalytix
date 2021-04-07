using System;
using System.Collections.Generic;
using System.Net;
using System.Text;

namespace CxAnalytix.Exceptions
{
	public class UnrecoverableOperationException : Exception
	{

		public UnrecoverableOperationException() : base("Unknown error")
		{
		}

		public UnrecoverableOperationException(String msg) : base(msg)
		{
		}

		public UnrecoverableOperationException(String msg, Exception ex) : base(msg, ex)
		{
		}

		public UnrecoverableOperationException(HttpStatusCode code, Uri url) 
			: base ($"Response code {Convert.ToInt32(code)}:{code} for request to {url}")
		{

		}

	}
}
