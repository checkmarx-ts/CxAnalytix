using CxAnalytix.TransformLogic;
using System;
using System.Collections.Generic;
using MongoDB.Bson;
using MongoDB.Driver;
using CxAnalytix.Configuration;
using log4net;

namespace CxAnalytix.Out.MongoDBOutput
{
    internal abstract class MongoDBOut : IOutput
    {
        private static ILog _log = LogManager.GetLogger(typeof(MongoDBOut));

        protected abstract IMongoCollection<BsonDocument> GetCollection();

        private BsonDocument BsonSerialize (IDictionary<string, object> record)
        {
            BsonDocument retVal = new BsonDocument();

            foreach (var key in record.Keys)
                retVal.Add(key.Replace ('.', '-'), BsonValue.Create(record[key]));

            return retVal;
        }

        public void write(IDictionary<string, object> record)
        {
            GetCollection ().InsertOne(BsonSerialize(record));
        }
    }
}
