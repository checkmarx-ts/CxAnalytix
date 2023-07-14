using MongoDB.Bson;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CxAnalytix.MongoTool
{
    internal static class Extensions
    {
        public static MongoUrl ChangeToDB(this MongoUrl src, String loginDBName) =>
            new MongoUrlBuilder(src.ToString()) { DatabaseName = loginDBName }.ToMongoUrl();

        public static IMongoDatabase GetDatabase(this MongoUrl src) => new MongoClient(src).GetDatabase(src.DatabaseName);

        public static IMongoDatabase GetDatabase(this MongoUrl src, String dbName) => new MongoClient(src).GetDatabase(dbName);


        public static bool CheckSuccess(this BsonDocument result) => result.First().Name.Equals("ok");

    }
}
