using CxAnalytix.Configuration;
using log4net;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using CxAnalytix.Interfaces.Outputs;
using CxAnalytix.Exceptions;

namespace CxAnalytix.Out.MongoDBOutput
{
	public sealed class MongoDBOutFactory : IOutputFactory
	{
		private static ILog _log = LogManager.GetLogger(typeof(MongoDBOutFactory));

		private static MongoOutConfig _outputConfig;
		internal static MongoOutConfig OutConfig { get => _outputConfig; }

		private static MongoConnectionConfig _conConfig;
		private static MongoClient _client;
		internal static MongoClient Client { get => _client; }


		private static IMongoDatabase _db;

		private static Dictionary<String, MongoDBOut> _schemas = new Dictionary<string, MongoDBOut>();
		internal MongoDBOut this[String name] => _schemas[name];

		private class Dummy : GenericSchema
		{
			public override void write(IClientSessionHandle session, IDictionary<string, object> record)
			{
			}
		}


		static MongoDBOutFactory()
		{
			try
			{
				_outputConfig = Config.GetConfig<MongoOutConfig>(MongoOutConfig.SECTION_NAME);
				_conConfig = Config.GetConfig<MongoConnectionConfig>(MongoConnectionConfig.SECTION_NAME);


				if (_log.IsDebugEnabled)
					foreach (var spec in _outputConfig.ShardKeys)
						_log.DebugFormat("Shard Key: {0}", spec);

				if (_outputConfig.ShardKeys.Count > 0)
					_log.InfoFormat("{0} calculated shard keys have been defined.", _outputConfig.ShardKeys.Count);

				MongoUrl mu = GetMongoConnectionString();

				_client = new MongoClient(mu);

				if (!_client.ListDatabaseNames().ToList().Contains(mu.DatabaseName))
					_log.Warn($"Database {mu.DatabaseName} does not exist, it will be created.");

				_db = _client.GetDatabase(mu.DatabaseName);

				// It is a violation of OOP principles for this component to know about these records.  At some point the schema
				// creation may be moved to an installer that initializes the DB prior to running the application.
				_schemas.Add(Config.Service.SASTScanDetailRecordName,
					MongoDBOut.CreateInstance<SastDetailSchema>(_db, Config.Service.SASTScanDetailRecordName, _outputConfig.ShardKeys[Config.Service.SASTScanDetailRecordName]));

				_schemas.Add(Config.Service.SASTScanSummaryRecordName, MongoDBOut.CreateInstance<SastSummarySchema>(_db, Config.Service.SASTScanSummaryRecordName,
					_outputConfig.ShardKeys[Config.Service.SASTScanSummaryRecordName]));

				_schemas.Add(Config.Service.SCAScanSummaryRecordName, MongoDBOut.CreateInstance<SCASummarySchema>(_db, Config.Service.SCAScanSummaryRecordName,
					_outputConfig.ShardKeys[Config.Service.SCAScanSummaryRecordName]));

				_schemas.Add(Config.Service.SCAScanDetailRecordName, MongoDBOut.CreateInstance<SCADetailSchema>(_db, Config.Service.SCAScanDetailRecordName,
					_outputConfig.ShardKeys[Config.Service.SCAScanDetailRecordName]));

				_schemas.Add(Config.Service.ProjectInfoRecordName, MongoDBOut.CreateInstance<ProjectInfoSchema>(_db, Config.Service.ProjectInfoRecordName,
					_outputConfig.ShardKeys[Config.Service.ProjectInfoRecordName]));

				_schemas.Add(Config.Service.PolicyViolationsRecordName, MongoDBOut.CreateInstance<PolicyViolationsSchema>(_db, Config.Service.PolicyViolationsRecordName,
					_outputConfig.ShardKeys[Config.Service.PolicyViolationsRecordName]));
			}
			catch (Exception ex)
			{
				_log.Error("Error initializing MongoDB connectivity.", ex);
				_client = null;
			}
		}

		private static MongoUrl GetMongoConnectionString()
		{
			MongoUrl mu = null;

			if (!String.IsNullOrEmpty(_outputConfig.ConnectionString))
			{
				mu = new MongoUrl(_outputConfig.ConnectionString);
				_log.Warn($"Using the ConnectionString attribute in the {MongoOutConfig.SECTION_NAME} configuration element is deprecated.  " +
					$"Please consider using the {MongoConnectionConfig.SECTION_NAME} element to configure the connection string.");
			}

			if (!String.IsNullOrEmpty(_conConfig.ConnectionString))
			{
				if (mu != null)
					_log.Warn($"Using the Mongo connecton string defined in the {MongoConnectionConfig.SECTION_NAME} configuration element, " +
						$"which overrides the connection string found in the {MongoOutConfig.SECTION_NAME} configuration element.");

				mu = new MongoUrl(_conConfig.ConnectionString);
			}

			return mu;
		}


		public IOutputTransaction StartTransaction()
		{
			if (Config.Service.EnablePseudoTransactions)
				return new MongoOutputTransaction(this);
			else
				return new MongoOutputNoTransaction(this);
		}

		public IRecordRef RegisterRecord(string recordName)
		{
			if (_client == null)
				throw new ProcessFatalException("The connection to MongoDB could not be established.");

			lock (_schemas)
				if (!_schemas.ContainsKey(recordName))
				{
					_schemas.Add(recordName, MongoDBOut.CreateInstance<GenericSchema>(_db, recordName, _outputConfig.ShardKeys[recordName]));

					return _schemas[recordName];
				}
				else
				{
					var dest = _schemas[recordName];

					if (!dest.VerifyOrCreateSchema())
					{
						_log.Warn($"Schema for {recordName} could not be verified or created, no data will be output for this record type.");
						return new Dummy();
					}

					return dest;
				}
		}
	}
}
