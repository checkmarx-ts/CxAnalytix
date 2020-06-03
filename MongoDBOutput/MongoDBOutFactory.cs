using CxAnalytix.Configuration;
using CxAnalytix.TransformLogic;
using log4net;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Text;

namespace CxAnalytix.Out.MongoDBOutput
{
    public sealed class MongoDBOutFactory : IOutputFactory
    {
        private static ILog _log = LogManager.GetLogger(typeof(MongoDBOutFactory));

        private static MongoOutConfig _cfg;
        private static MongoClient _client;
        private static IMongoDatabase _db;

        private static Dictionary<String, ISchema> _schemas = new Dictionary<string, ISchema>();

        private class Dummy : IOutput
        {
            public void write(IDictionary<string, object> record)
            {
            }
        }


        static MongoDBOutFactory()
        {
            _cfg = Config.GetConfig<MongoOutConfig>(MongoOutConfig.SECTION_NAME);
            _client = new MongoClient(_cfg.ConnectionString);

            if (!_client.ListDatabaseNames().ToList().Contains(_cfg.DBName))
                _log.Warn($"Database {_cfg.DBName} does not exists, it will be created.");

            _db = _client.GetDatabase(_cfg.DBName);

            _schemas.Add(Config.Service.SASTScanDetailRecordName, MongoDBOut.CreateInstance<SastDetailSchema>(_db, Config.Service.SASTScanDetailRecordName) );
            _schemas.Add(Config.Service.SASTScanSummaryRecordName, MongoDBOut.CreateInstance<SastSummarySchema>(_db, Config.Service.SASTScanSummaryRecordName) );
            _schemas.Add(Config.Service.SCAScanSummaryRecordName, MongoDBOut.CreateInstance <SCASummarySchema> (_db, Config.Service.SCAScanSummaryRecordName) );
            _schemas.Add(Config.Service.SCAScanDetailRecordName, MongoDBOut.CreateInstance <SCADetailSchema>(_db, Config.Service.SCAScanDetailRecordName) );
            //_schemas.Add(Config.Service.ProjectInfoRecordName, );
            //_schemas.Add(Config.Service.PolicyViolationsRecordName, );
        }


        public IOutput newInstance(string recordType)
        {
            if (!_schemas.ContainsKey(recordType))
            {
                _log.Warn($"Schema for {recordType} not found, no data will be output for this record type.");

                return new Dummy();
            }
            else
            {
                var dest = _schemas[recordType];

                if (!dest.VerifyOrCreateSchema())
                    _log.Warn($"Schema for {recordType} could not be verified or created, no data will be output for this record type.");

                return dest;
            }
        }
    }
}
