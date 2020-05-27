using CxAnalytix.Configuration;
using CxAnalytix.TransformLogic;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Text;

namespace CxAnalytix.Out.MongoDBOutput
{
    public sealed class MongoDBOutFactory : IOutputFactory
    {

        static MongoOutConfig _cfg;
        static MongoClient _client;
        static IMongoDatabase _db;



        static MongoDBOutFactory()
        {
            _cfg = Config.GetConfig<MongoOutConfig>(MongoOutConfig.SECTION_NAME);
            _client = new MongoClient(_cfg.ConnectionString);
            _db = _client.GetDatabase(_cfg.DBName);
        }


        public IOutput newInstance(string recordType)
        {
            return new MongoDBOut(_db, recordType);
        }
    }
}
