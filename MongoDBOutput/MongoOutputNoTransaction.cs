using CxAnalytix.Interfaces.Outputs;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Text;

namespace CxAnalytix.Out.MongoDBOutput
{
	internal class MongoOutputNoTransaction : MongoOutputBase
	{

		IClientSessionHandle _session = MongoDBOutFactory.Client.StartSession();

		internal MongoOutputNoTransaction (MongoDBOutFactory instance) : base(instance)
		{

		}


		public override void Dispose()
		{
			if (_session != null)
			{
				_session.Dispose();
				_session = null;
			}
		}

		public override void write(IRecordRef which, IDictionary<string, object> record)
		{
			this[which.RecordName].write(_session, record);
		}
	}
}
