using CxAnalytix.TransformLogic;
using System;
using System.Collections.Generic;
using MongoDB.Bson;
using MongoDB.Driver;
using CxAnalytix.Configuration;
using log4net;

namespace CxAnalytix.Out.MongoDBOutput
{
    internal sealed class MongoDBOut : IOutput
    {
        public String CollectionName { get; private set; }

        private static ILog _log = LogManager.GetLogger(typeof(MongoDBOut));


        public MongoDBOut(String collectionName)
        {
            CollectionName = collectionName;
        }

        private BsonDocument BsonSerialize (IDictionary<string, object> record)
        {
            BsonDocument retVal = new BsonDocument();

            foreach (var key in record.Keys)
            {
                retVal.Add(key.Replace ('.', '-'), BsonValue.Create(record[key]));
            }


            return retVal;
        }



        public void write(IDictionary<string, object> record)
        {
            var cfg = Config.GetConfig<MongoOutConfig>(MongoOutConfig.SECTION_NAME);

            var client = new MongoClient(cfg.ConnectionString);
            var database = client.GetDatabase("test");
            var collection = database.GetCollection<BsonDocument>(CollectionName);

            collection.InsertOne(BsonSerialize(record));
        }
    }
}
