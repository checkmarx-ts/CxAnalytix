using log4net;
using MongoDB.Bson;
using MongoDB.Driver;
using System;

namespace CxAnalytix.Out.MongoDBOutput
{
    internal class SCADetailSchema : MongoDBOut, ISchema
    {
        private static ILog _log = LogManager.GetLogger(typeof(SCADetailSchema));

        public override bool VerifyOrCreateSchema()
        {

            var opts = new CreateIndexOptions()
            {
                Background = true
            };

            opts.Name = "ProjectName-A+ScanId-A";
            if (!MongoUtil.IndexExists(Collection, opts.Name))
                Collection.Indexes.CreateOne(new CreateIndexModel<BsonDocument>(Builders<BsonDocument>.IndexKeys
                    .Ascending(new StringFieldDefinition<BsonDocument, String>("ProjectName"))
                    .Ascending(new StringFieldDefinition<BsonDocument, String>("ScanId"))
                    , opts));

            opts.Name = "LibraryName-A+LibraryVersion-A";
            if (!MongoUtil.IndexExists(Collection, opts.Name))
                Collection.Indexes.CreateOne(new CreateIndexModel<BsonDocument>(Builders<BsonDocument>.IndexKeys
                    .Ascending(new StringFieldDefinition<BsonDocument, String>("LibraryName"))
                    .Ascending(new StringFieldDefinition<BsonDocument, String>("LibraryVersion"))
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
