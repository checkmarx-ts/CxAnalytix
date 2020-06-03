using System;
using CxAnalytix.TransformLogic;
using System.Collections.Generic;
using MongoDB.Bson;
using MongoDB.Driver;
using log4net;


namespace CxAnalytix.Out.MongoDBOutput
{
    internal abstract class MongoDBOut : IOutput
    {
        private static ILog _log = LogManager.GetLogger(typeof(MongoDBOut));

        protected IMongoCollection<BsonDocument> Collection { get; private set; }
        protected IMongoDatabase DB { get; private set; }


        protected MongoDBOut ()
        { }

        public static T CreateInstance<T>(IMongoDatabase db, String collectionName) where T : MongoDBOut, new()
        {
            T retVal = new T();
            retVal.DB = db;
            retVal.Collection = MongoUtil.MakeCollection(db, collectionName);

            return retVal;
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
            Collection.InsertOne(BsonSerialize(record));
        }
    }
}
