using log4net;
using MongoDB.Bson;
using MongoDB.Driver;
using System;

namespace CxAnalytix.Out.MongoDBOutput
{
    internal class ProjectInfoSchema : MongoDBOut, ISchema
    {
        private static ILog _log = LogManager.GetLogger(typeof(ProjectInfoSchema));

        public bool VerifyOrCreateSchema()
        {

            var opts = new CreateIndexOptions()
            {
                Background = true
            };

            opts.Name = "ProjectName-A";
            if (!MongoUtil.IndexExists(Collection, opts.Name))
                Collection.Indexes.CreateOne(new CreateIndexModel<BsonDocument>(Builders<BsonDocument>.IndexKeys
                    .Ascending(new StringFieldDefinition<BsonDocument, String>("ProjectName"))
                    , opts));

            opts.Name = "SAST_LastScanDate-A";
            if (!MongoUtil.IndexExists(Collection, opts.Name))
                Collection.Indexes.CreateOne(new CreateIndexModel<BsonDocument>(Builders<BsonDocument>.IndexKeys
                    .Ascending(new StringFieldDefinition<BsonDocument, DateTime>("SAST_LastScanDate"))
                    , opts));

            opts.Name = "SCA_LastScanDate-A";
            if (!MongoUtil.IndexExists(Collection, opts.Name))
                Collection.Indexes.CreateOne(new CreateIndexModel<BsonDocument>(Builders<BsonDocument>.IndexKeys
                    .Ascending(new StringFieldDefinition<BsonDocument, DateTime>("SCA_LastScanDate"))
                    , opts));


            return true;
        }

    }
}
