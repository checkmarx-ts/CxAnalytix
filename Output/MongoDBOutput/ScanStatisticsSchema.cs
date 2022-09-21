using log4net;
using MongoDB.Bson;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CxAnalytix.Out.MongoDBOutput
{
    internal class ScanStatisticsSchema : MongoDBOut, ISchema
    {
        private static ILog _log = LogManager.GetLogger(typeof(ProjectInfoSchema));

        public override bool VerifyOrCreateSchema()
        {
            var opts = new CreateIndexOptions()
            {
                Background = true
            };

            opts.Name = "Language-A";
            if (!MongoUtil.IndexExists(Collection, opts.Name))
                Collection.Indexes.CreateOne(new CreateIndexModel<BsonDocument>(Builders<BsonDocument>.IndexKeys
                    .Ascending(new StringFieldDefinition<BsonDocument, String>("Language"))
                    , opts));

            return true;
        }
    }
}
