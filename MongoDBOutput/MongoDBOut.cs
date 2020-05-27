using CxAnalytix.TransformLogic;
using System;
using System.Collections.Generic;
using MongoDB.Bson;
using MongoDB.Driver;
using CxAnalytix.Configuration;
using log4net;

namespace CxAnalytix.Out.MongoDBOutput
{
    internal class MongoDBOut : IOutput
    {

        /*
         TODO: Subclass this, one per collection.  reverse which collection type it is using the config and name
         if collection doesn't exist, make it and add indexes in the specific class
         need configs for each collection type to add shard key field as a calculated hash
             
        */


        public String CollectionName { get; private set; }

        private static ILog _log = LogManager.GetLogger(typeof(MongoDBOut));

        private IMongoDatabase _db;

        private IMongoCollection<BsonDocument> _coll;

        public MongoDBOut(IMongoDatabase db, String collectionName)
        {
            CollectionName = collectionName;
            _db = db;

            var names = _db.ListCollectionNames();

            if (!names.ToList ().Contains (CollectionName) )
                _db.CreateCollection(CollectionName);

            _coll = _db.GetCollection<BsonDocument>(CollectionName);
        }

        private BsonDocument BsonSerialize (IDictionary<string, object> record)
        {
            BsonDocument retVal = new BsonDocument();

            foreach (var key in record.Keys)
                retVal.Add(key.Replace ('.', '-'), BsonValue.Create(record[key]));

            return retVal;
        }

        public void write(IDictionary<string, object> record)
        {
            _coll.InsertOne(BsonSerialize(record));
        }
    }
}
