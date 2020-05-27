using CxAnalytix.TransformLogic;
using System;
using System.Collections.Generic;
using MongoDB.Bson;
using MongoDB.Driver;
using CxAnalytix.Configuration;

namespace CxAnalytix.Out.MongoDBOutput
{
    internal sealed class MongoDBOut : IOutput
    {
        public String CollectionName { get; private set; }


        public MongoDBOut(String collectionName)
        {
            CollectionName = collectionName;
        }

        public void write(IDictionary<string, object> record)
        {

            var cfg = Config.GetConfig<MongoOutConfig>(MongoOutConfig.SECTION_NAME);

            var client = new MongoClient(cfg.ConnectionString);
            var database = client.GetDatabase("test");
            var collection = database.GetCollection<BsonDocument>(CollectionName);

            object dt = DateTime.Now;
            BsonValue bv = BsonValue.Create(dt);

            collection.InsertOne(new BsonDocument(new BsonDocument("DateStamp", bv)));

        }
    }
}
