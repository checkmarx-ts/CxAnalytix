using log4net;
using MongoDB.Bson;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Text;

namespace CxAnalytix.Out.MongoDBOutput
{
    internal class MongoUtil
    {

        private static ILog _log = LogManager.GetLogger(typeof(MongoUtil));

        public static bool CollectionExists (IMongoDatabase db, String collectionName)
        {
            var names = db.ListCollectionNames();

            return names.ToList().Contains(collectionName);
        }

        public static IMongoCollection<BsonDocument> MakeCollection(IMongoDatabase db, String collectionName)
        {
            if (!MongoUtil.CollectionExists(db, collectionName))
            {
                _log.Info($"Creating collection {collectionName}");

                db.CreateCollection(collectionName);
            }


            return db.GetCollection<BsonDocument>(collectionName);
        }



        public static bool IndexExists (IMongoCollection<BsonDocument> coll, String indexName)
        {

            var indexNames = coll.Indexes.List().ToList();

            foreach (var bsonDoc in coll.Indexes.List().ToList())
                if (bsonDoc.Contains("name") && bsonDoc["name"].CompareTo(indexName) == 0)
                    return true;


            return false;
        }
    }
}
