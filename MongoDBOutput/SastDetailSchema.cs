using log4net;
using MongoDB.Bson;
using MongoDB.Driver;
using System;

namespace CxAnalytix.Out.MongoDBOutput
{
    internal class SastDetailSchema : MongoDBOut, ISchema
    {

        private static ILog _log = LogManager.GetLogger(typeof(SastDetailSchema));
        private IMongoCollection<BsonDocument> _coll = null;

        public bool VerifyOrCreateSchema(IMongoDatabase db, String collectionName)
        {
            _coll = MongoUtil.MakeCollection(db, collectionName);

            var opts = new CreateIndexOptions()
            {
                Background = true
            };

            opts.Name = "ProjectName-A+ScanId-D";
            if (!MongoUtil.IndexExists (_coll, opts.Name) )
            _coll.Indexes.CreateOne(new CreateIndexModel<BsonDocument>(Builders<BsonDocument>.IndexKeys
                .Ascending(new StringFieldDefinition<BsonDocument, String>("ProjectName"))
                .Descending(new StringFieldDefinition<BsonDocument, Int32>("ScanId"))
                , opts));

            opts.Name = "ScanId-D+PathId-A";
            if (!MongoUtil.IndexExists(_coll, opts.Name))
                _coll.Indexes.CreateOne(new CreateIndexModel<BsonDocument>(Builders<BsonDocument>.IndexKeys
                .Descending(new StringFieldDefinition<BsonDocument, Int32>("ScanId"))
                .Ascending(new StringFieldDefinition<BsonDocument, Int32>("PathId"))
                , opts));

            opts.Name = "SimilarityId-D+ProjectName-A";
            if (!MongoUtil.IndexExists(_coll, opts.Name))
                _coll.Indexes.CreateOne(new CreateIndexModel<BsonDocument>(Builders<BsonDocument>.IndexKeys
                .Descending(new StringFieldDefinition<BsonDocument, String>("SimilarityId"))
                .Ascending(new StringFieldDefinition<BsonDocument, String>("ProjectName"))
                , opts));

            opts.Name = "SinkFileName-A";
            if (!MongoUtil.IndexExists(_coll, opts.Name))
                _coll.Indexes.CreateOne(new CreateIndexModel<BsonDocument>(Builders<BsonDocument>.IndexKeys
                .Ascending(new StringFieldDefinition<BsonDocument, String>("SinkFileName"))
                , opts));

            opts.Name = "QueryName-A";
            if (!MongoUtil.IndexExists(_coll, opts.Name))
                _coll.Indexes.CreateOne(new CreateIndexModel<BsonDocument>(Builders<BsonDocument>.IndexKeys
                .Ascending(new StringFieldDefinition<BsonDocument, String>("QueryName"))
                , opts));

            opts.Name = "ResultSeverity-A";
            if (!MongoUtil.IndexExists(_coll, opts.Name))
                _coll.Indexes.CreateOne(new CreateIndexModel<BsonDocument>(Builders<BsonDocument>.IndexKeys
                .Ascending(new StringFieldDefinition<BsonDocument, String>("ResultSeverity"))
                , opts));

            opts.Name = "QueryLanguage-A";
            if (!MongoUtil.IndexExists(_coll, opts.Name))
                _coll.Indexes.CreateOne(new CreateIndexModel<BsonDocument>(Builders<BsonDocument>.IndexKeys
                .Ascending(new StringFieldDefinition<BsonDocument, String>("QueryLanguage"))
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
