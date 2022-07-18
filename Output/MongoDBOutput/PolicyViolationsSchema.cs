using log4net;
using MongoDB.Bson;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Text;

namespace CxAnalytix.Out.MongoDBOutput
{
    internal class PolicyViolationsSchema : MongoDBOut, ISchema
    {
        private static ILog _log = LogManager.GetLogger(typeof(PolicyViolationsSchema));

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

            opts.Name = "FirstViolationDetectionDate-A";
            if (!MongoUtil.IndexExists(Collection, opts.Name))
                Collection.Indexes.CreateOne(new CreateIndexModel<BsonDocument>(Builders<BsonDocument>.IndexKeys
                    .Ascending(new StringFieldDefinition<BsonDocument, DateTime>("FirstViolationDetectionDate"))
                    , opts));

            opts.Name = "ViolationOccurredDate-A";
            if (!MongoUtil.IndexExists(Collection, opts.Name))
                Collection.Indexes.CreateOne(new CreateIndexModel<BsonDocument>(Builders<BsonDocument>.IndexKeys
                    .Ascending(new StringFieldDefinition<BsonDocument, DateTime>("ViolationOccurredDate"))
                    , opts));

            return true;
        }

    }
}
