using System;

namespace CxAnalytix.Exceptions
{
	public class ProcessFatalException : Exception
	{

		public ProcessFatalException(string? message) : base(message)
		{

		}

		public ProcessFatalException(string? message, Exception? innerException) : base (message, innerException)
		{

		}


	}
}
