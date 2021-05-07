using System;
using System.Collections.Generic;
using MongoDB.Bson;
using MongoDB.Driver;
using log4net;
using CxAnalytix.Extensions;
using System.Security.Cryptography;
using CxAnalytix.Interfaces.Outputs;
using System.IO;

namespace CxAnalytix.Out.MongoDBOutput
{
    internal abstract class MongoDBOut : ISchema
    {
        private static ILog _log = LogManager.GetLogger(typeof(MongoDBOut));

        public IMongoCollection<BsonDocument> Collection { get; private set; }
        protected IMongoDatabase DB { get; private set; }

        protected ShardKeySpec Spec { get; set; }

		public string RecordName { get; private set; }

		private SHA256 _sha = SHA256.Create();

        protected MongoDBOut ()
        { }

        public static T CreateInstance<T>(IMongoDatabase db, String collectionName, ShardKeySpec spec) where T : MongoDBOut, new()
        {
            T retVal = new T();
            retVal.DB = db;
            retVal.Collection = MongoUtil.MakeCollection(db, collectionName);
            retVal.Spec = spec;
            retVal.RecordName = collectionName;

            return retVal;
        }

        private BsonDocument ToBsonDocument (IDictionary<string, object> record)
        {
            BsonDocument retVal = new BsonDocument();

            foreach (var key in record.Keys)
                retVal.Add(key.Replace('.', '-'), BsonValue.Create(record[key]));

            return retVal;
        }

        public BsonDocument BsonSerialize (IDictionary<string, object> record)
		{
            AddExtraFields(record);
            return ToBsonDocument(record);
        }


        private void AddExtraFields(IDictionary<string, object> record)
		{
            if (Spec != null)
            {
                String keyValue = record.ComposeString(Spec.Format);

                if (!Spec.NoHash)
                    keyValue = Convert.ToBase64String(_sha.ComputeHash(System.Text.UTF8Encoding.UTF8.GetBytes(keyValue)));

                record.Add(Spec.Key, keyValue);
            }

            record.Add("_inserted", DateTime.Now.ToUniversalTime());

        }

        public virtual void write(IClientSessionHandle session, IDictionary<string, object> record)
        {
            AddExtraFields(record);
            Collection.InsertOne(session, ToBsonDocument(record));
        }

        public virtual void write(IClientSessionHandle session, IEnumerable<BsonDocument> docs)
        {
            Collection.InsertMany(session, docs);
        }

        public abstract bool VerifyOrCreateSchema();
	}
}
