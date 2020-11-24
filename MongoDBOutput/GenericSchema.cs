using log4net;
using System;
using System.Collections.Generic;
using System.Text;

namespace CxAnalytix.Out.MongoDBOutput
{
	internal class GenericSchema : MongoDBOut, ISchema
	{
		private static ILog _log = LogManager.GetLogger(typeof(GenericSchema));

		public bool VerifyOrCreateSchema()
		{
			return true;
		}
	}
}
