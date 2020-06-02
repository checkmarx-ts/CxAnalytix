using System;
using System.Collections.Generic;
using System.Text;
using log4net;
using MongoDB.Bson;
using MongoDB.Driver;

namespace CxAnalytix.Out.MongoDBOutput
{
    internal class SastSummarySchema : MongoDBOut, ISchema
    {
        private static ILog _log = LogManager.GetLogger(typeof(SastSummarySchema));
        private IMongoCollection<BsonDocument> _coll = null;

        public bool VerifyOrCreateSchema(IMongoDatabase db, string collectionName)
        {
            _coll = MongoUtil.MakeCollection(db, collectionName);

            var opts = new CreateIndexOptions()
            {
                Background = true
            };

            opts.Name = "ProjectName-A+ScanId-D";
            if (!MongoUtil.IndexExists(_coll, opts.Name))
                _coll.Indexes.CreateOne(new CreateIndexModel<BsonDocument>(Builders<BsonDocument>.IndexKeys
                    .Ascending(new StringFieldDefinition<BsonDocument, String>("ProjectName"))
                    .Descending(new StringFieldDefinition<BsonDocument, String>("ScanId"))
                    , opts));

            opts.Name = "High-A";
            if (!MongoUtil.IndexExists(_coll, opts.Name))
                _coll.Indexes.CreateOne(new CreateIndexModel<BsonDocument>(Builders<BsonDocument>.IndexKeys
                    .Ascending(new StringFieldDefinition<BsonDocument, Int64>("High"))
                    , opts));

            opts.Name = "Medium-A";
            if (!MongoUtil.IndexExists(_coll, opts.Name))
                _coll.Indexes.CreateOne(new CreateIndexModel<BsonDocument>(Builders<BsonDocument>.IndexKeys
                    .Ascending(new StringFieldDefinition<BsonDocument, Int64>("Medium"))
                    , opts));

            opts.Name = "Low-A";
            if (!MongoUtil.IndexExists(_coll, opts.Name))
                _coll.Indexes.CreateOne(new CreateIndexModel<BsonDocument>(Builders<BsonDocument>.IndexKeys
                    .Ascending(new StringFieldDefinition<BsonDocument, Int64>("Low"))
                    , opts));

            opts.Name = "Information-A";
            if (!MongoUtil.IndexExists(_coll, opts.Name))
                _coll.Indexes.CreateOne(new CreateIndexModel<BsonDocument>(Builders<BsonDocument>.IndexKeys
                    .Ascending(new StringFieldDefinition<BsonDocument, Int64>("Information"))
                    , opts));

            opts.Name = "ScanFinished-A";
            if (!MongoUtil.IndexExists(_coll, opts.Name))
                _coll.Indexes.CreateOne(new CreateIndexModel<BsonDocument>(Builders<BsonDocument>.IndexKeys
                    .Ascending(new StringFieldDefinition<BsonDocument, DateTime>("ScanFinished"))
                    , opts));


            return true;
        }

        protected override IMongoCollection<BsonDocument> GetCollection()
        {
            if (_coll == null)
                throw new InvalidOperationException("The class was not initialized correctly.");

            return _coll;
        }
    }
}
