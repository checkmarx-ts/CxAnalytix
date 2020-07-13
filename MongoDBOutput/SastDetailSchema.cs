using log4net;
using MongoDB.Bson;
using MongoDB.Driver;
using System;

namespace CxAnalytix.Out.MongoDBOutput
{
    internal class SastDetailSchema : MongoDBOut, ISchema
    {
        private static ILog _log = LogManager.GetLogger(typeof(SastDetailSchema));

        public bool VerifyOrCreateSchema()
        {
            var opts = new CreateIndexOptions()
            {
                Background = true
            };

            opts.Name = "ProjectName-A+ScanId-D";
            if (!MongoUtil.IndexExists (Collection, opts.Name) )
            Collection.Indexes.CreateOne(new CreateIndexModel<BsonDocument>(Builders<BsonDocument>.IndexKeys
                .Ascending(new StringFieldDefinition<BsonDocument, String>("ProjectName"))
                .Descending(new StringFieldDefinition<BsonDocument, String>("ScanId"))
                , opts));

            opts.Name = "ScanId-D+PathId-A";
            if (!MongoUtil.IndexExists(Collection, opts.Name))
                Collection.Indexes.CreateOne(new CreateIndexModel<BsonDocument>(Builders<BsonDocument>.IndexKeys
                .Descending(new StringFieldDefinition<BsonDocument, String>("ScanId"))
                .Ascending(new StringFieldDefinition<BsonDocument, String>("PathId"))
                , opts));

            opts.Name = "SimilarityId-D+ProjectName-A";
            if (!MongoUtil.IndexExists(Collection, opts.Name))
                Collection.Indexes.CreateOne(new CreateIndexModel<BsonDocument>(Builders<BsonDocument>.IndexKeys
                .Descending(new StringFieldDefinition<BsonDocument, String>("SimilarityId"))
                .Ascending(new StringFieldDefinition<BsonDocument, String>("ProjectName"))
                , opts));

            opts.Name = "SinkFileName-A";
            if (!MongoUtil.IndexExists(Collection, opts.Name))
                Collection.Indexes.CreateOne(new CreateIndexModel<BsonDocument>(Builders<BsonDocument>.IndexKeys
                .Ascending(new StringFieldDefinition<BsonDocument, String>("SinkFileName"))
                , opts));

            opts.Name = "QueryName-A";
            if (!MongoUtil.IndexExists(Collection, opts.Name))
                Collection.Indexes.CreateOne(new CreateIndexModel<BsonDocument>(Builders<BsonDocument>.IndexKeys
                .Ascending(new StringFieldDefinition<BsonDocument, String>("QueryName"))
                , opts));

            opts.Name = "ResultSeverity-A";
            if (!MongoUtil.IndexExists(Collection, opts.Name))
                Collection.Indexes.CreateOne(new CreateIndexModel<BsonDocument>(Builders<BsonDocument>.IndexKeys
                .Ascending(new StringFieldDefinition<BsonDocument, String>("ResultSeverity"))
                , opts));

            opts.Name = "QueryLanguage-A";
            if (!MongoUtil.IndexExists(Collection, opts.Name))
                Collection.Indexes.CreateOne(new CreateIndexModel<BsonDocument>(Builders<BsonDocument>.IndexKeys
                .Ascending(new StringFieldDefinition<BsonDocument, String>("QueryLanguage"))
                , opts));

            opts.Name = "ScanFinished-A";
            if (!MongoUtil.IndexExists(Collection, opts.Name))
                Collection.Indexes.CreateOne(new CreateIndexModel<BsonDocument>(Builders<BsonDocument>.IndexKeys
                    .Ascending(new StringFieldDefinition<BsonDocument, DateTime>("ScanFinished"))
                    , opts));


            return true;
        }
    }
}
