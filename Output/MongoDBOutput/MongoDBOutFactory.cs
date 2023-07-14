using log4net;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using CxAnalytix.Interfaces.Outputs;
using CxAnalytix.Exceptions;
using CxAnalytix.Configuration.Impls;
using CxAnalytix.Out.MongoDBOutput.Config.Impl;
using System.Runtime.CompilerServices;

namespace CxAnalytix.Out.MongoDBOutput
{
    public sealed class MongoDBOutFactory : SDK.Modules.Output.OutputModule
	{
		private static ILog _log = LogManager.GetLogger(typeof(MongoDBOutFactory));

		internal static MongoOutConfig OutConfig => CxAnalytix.Configuration.Impls.Config.GetConfig<MongoOutConfig>();

		internal static MongoConnectionConfig ConConfig => CxAnalytix.Configuration.Impls.Config.GetConfig<MongoConnectionConfig>();

		private static CxAnalytixService Service => CxAnalytix.Configuration.Impls.Config.GetConfig<CxAnalytixService>();


		[MethodImpl(MethodImplOptions.Synchronized)]
		private static void Init()
        {
			if (_client != null)
				return;

			try
			{
				if (_log.IsDebugEnabled)
					foreach (var spec in OutConfig.ShardKeys)
						_log.DebugFormat("Shard Key: {0}", spec);

				if (OutConfig.ShardKeys.Count > 0)
					_log.InfoFormat("{0} calculated shard keys have been defined.", OutConfig.ShardKeys.Count);

				MongoUrl mu = GetMongoConnectionString();

				_client = new MongoClient(mu);

				if (!_client.ListDatabaseNames().ToList().Contains(mu.DatabaseName))
					throw new ProcessFatalException($"Database {mu.DatabaseName} does not exist, did you forget to run the CxAnalytix MongoTool?");

				_db = _client.GetDatabase(mu.DatabaseName);
            }
            catch (Exception ex)
			{
				_log.Error("Error initializing MongoDB connectivity.", ex);
				_client = null;
                throw;
            }
		}

		private static MongoClient _client;
		internal static MongoClient Client { 
			get
            {
				if (_client == null)
					Init();

				return _client;
            } 
		}


		private static IMongoDatabase _db;

		private static Dictionary<String, MongoDBOut> _schemas = new Dictionary<string, MongoDBOut>();
		internal MongoDBOut this[String name] => _schemas[name];

		private class Dummy : GenericSchema
		{
			public override void write(IClientSessionHandle session, IDictionary<string, object> record)
			{
			}
		}


        public MongoDBOutFactory() : base("MongoDB", typeof(MongoDBOutFactory))
		{
		}

		private static MongoUrl GetMongoConnectionString() => new MongoUrl(ConConfig.ConnectionString);

		public override IOutputTransaction StartTransaction()
		{
			if (Service.EnablePseudoTransactions)
				return new MongoOutputTransaction(this);
			else
				return new MongoOutputNoTransaction(this);
		}

		public override IRecordRef RegisterRecord(string recordName)
		{
			if (Client == null)
				throw new ProcessFatalException("The connection to MongoDB could not be established.");

			lock (_schemas)
			{
				if (!_schemas.ContainsKey(recordName))
					_schemas.Add(recordName, MongoDBOut.CreateInstance<GenericSchema>(_db, recordName, OutConfig.ShardKeys[recordName]));
            
				return _schemas[recordName];
            }
		}
	}
}
