﻿using CxAnalytix.Configuration;
using log4net;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using CxAnalytix.Interfaces.Outputs;
using CxAnalytix.Exceptions;
using CxAnalytix.Out.MongoDBOutput.Config.Impl;
using CxAnalytix.Out.MongoDBOutput.Config.Contracts;
using System.Reflection;
using CxAnalytix.Configuration.Contracts;

namespace CxAnalytix.Out.MongoDBOutput
{
    public sealed class MongoDBOutFactory : IOutputFactory
	{
		private static ILog _log = LogManager.GetLogger(typeof(MongoDBOutFactory));

		internal static IMongoOutConfig OutConfig { get; set; }

		internal static IMongoConnectionConfig ConConfig { get; set; }

		private static ICxAnalytixService Service { get; set; }


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
				OutConfig = CxAnalytix.Configuration.Impls.Config.GetConfig<IMongoOutConfig>();
				ConConfig = CxAnalytix.Configuration.Impls.Config.GetConfig<IMongoConnectionConfig>();
				Service = CxAnalytix.Configuration.Impls.Config.GetConfig<ICxAnalytixService>();


				if (_log.IsDebugEnabled)
					foreach (var spec in OutConfig.ShardKeys)
						_log.DebugFormat("Shard Key: {0}", spec);

				if (OutConfig.ShardKeys.Count > 0)
					_log.InfoFormat("{0} calculated shard keys have been defined.", OutConfig.ShardKeys.Count);

				MongoUrl mu = GetMongoConnectionString();

				_client = new MongoClient(mu);

				if (!_client.ListDatabaseNames().ToList().Contains(mu.DatabaseName))
					_log.Warn($"Database {mu.DatabaseName} does not exist, it will be created.");

				_db = _client.GetDatabase(mu.DatabaseName);

				// It is a violation of OOP principles for this component to know about these records.  At some point the schema
				// creation may be moved to an installer that initializes the DB prior to running the application.
				_schemas.Add(Service.SASTScanDetailRecordName,
					MongoDBOut.CreateInstance<SastDetailSchema>(_db, Service.SASTScanDetailRecordName, OutConfig.ShardKeys[Service.SASTScanDetailRecordName]));

				_schemas.Add(Service.SASTScanSummaryRecordName, MongoDBOut.CreateInstance<SastSummarySchema>(_db, Service.SASTScanSummaryRecordName,
					OutConfig.ShardKeys[Service.SASTScanSummaryRecordName]));

				_schemas.Add(Service.SCAScanSummaryRecordName, MongoDBOut.CreateInstance<SCASummarySchema>(_db, Service.SCAScanSummaryRecordName,
					OutConfig.ShardKeys[Service.SCAScanSummaryRecordName]));

				_schemas.Add(Service.SCAScanDetailRecordName, MongoDBOut.CreateInstance<SCADetailSchema>(_db, Service.SCAScanDetailRecordName,
					OutConfig.ShardKeys[Service.SCAScanDetailRecordName]));

				_schemas.Add(Service.ProjectInfoRecordName, MongoDBOut.CreateInstance<ProjectInfoSchema>(_db, Service.ProjectInfoRecordName,
					OutConfig.ShardKeys[Service.ProjectInfoRecordName]));

				_schemas.Add(Service.PolicyViolationsRecordName, MongoDBOut.CreateInstance<PolicyViolationsSchema>(_db, Service.PolicyViolationsRecordName,
					OutConfig.ShardKeys[Service.PolicyViolationsRecordName]));
			}
			catch (Exception ex)
			{
				_log.Error("Error initializing MongoDB connectivity.", ex);
				_client = null;
			}
		}

		private static MongoUrl GetMongoConnectionString() => new MongoUrl(ConConfig.ConnectionString);

		public IOutputTransaction StartTransaction()
		{
			if (Service.EnablePseudoTransactions)
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
					_schemas.Add(recordName, MongoDBOut.CreateInstance<GenericSchema>(_db, recordName, OutConfig.ShardKeys[recordName]));

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