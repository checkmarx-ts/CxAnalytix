using CxAnalytix.Interfaces.Outputs;
using log4net;
using System;
using System.Collections.Generic;
using System.Text;

namespace CxAnalytix.Out.MongoDBOutput
{
	internal abstract class MongoOutputBase : IOutputTransaction
	{

		private MongoDBOutFactory _inst;

		private static ILog _log = LogManager.GetLogger(typeof(MongoOutputBase));

		private Guid _id = Guid.NewGuid();

		public string TransactionId => _id.ToString();

		internal MongoDBOut this[String name] => _inst[name];


		internal MongoOutputBase (MongoDBOutFactory instance)
		{
			_inst = instance;

		}

		public virtual bool Commit()
		{
			return true;
		}

		public abstract void Dispose();

		public virtual void write(IRecordRef which, IDictionary<string, object> record)
		{
			throw new NotImplementedException();
		}
	}
}
