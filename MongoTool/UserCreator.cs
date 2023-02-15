using CxAnalytix.Exceptions;
using log4net;
using MongoDB.Bson;
using MongoDB.Driver;
using MongoDB.Driver.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CxAnalytix.MongoTool
{
    internal static class UserCreator
    {
        private static ILog _log = LogManager.GetLogger(typeof(UserCreator));

        public static void AssignRoles(MongoUrl mongoEndpoint, String loginDBName, String runtimeUser, String runtimePassword)
        {
        }

        private static IMongoCollection<BsonDocument> GetUsersCollection(MongoUrl loginDBUrl)
        {
            var loginDB = loginDBUrl.GetDatabase();

            if (String.IsNullOrEmpty(loginDB.ListCollectionNames().ToEnumerable().FirstOrDefault(name => name.Equals("system.users"))))
                throw new ProcessFatalException($"Cannot find [system.users] in the login DB [{loginDBUrl.DatabaseName}].");

            return loginDB.GetCollection<BsonDocument>("system.users");
        }

        public static void CreateOrUpdateUser(MongoUrl mongoEndpoint, String loginDBName)
        {
            CreateOrUpdateUser(mongoEndpoint, loginDBName, mongoEndpoint.Username, mongoEndpoint.Password);
        }


        public static void CreateOrUpdateUser(MongoUrl mongoEndpoint, String loginDBName, String targetUser, String targetPassword)
        {
            var loginEndpoint = mongoEndpoint.ChangeToDB(loginDBName);

            var filter = new BsonDocument() { { "user", new BsonDocument() { { "$eq", targetUser } } },
                { "db", new BsonDocument() { { "$eq", mongoEndpoint.DatabaseName } }  } };

            var findResult = GetUsersCollection(loginEndpoint).Find<BsonDocument>(filter);

            BsonDocument command = null;
            if (findResult.CountDocuments() == 0)
            {
                _log.Info($"Creating user [{mongoEndpoint.DatabaseName}.{targetUser}]...");

                command = new BsonDocument() { { "createUser", targetUser }, { "pwd", targetPassword},
                { "roles", new BsonArray() { new BsonDocument() { { "role", "readWrite" }, { "db", mongoEndpoint.DatabaseName } } } } };

            }
            else
            {
                _log.Info($"User [{mongoEndpoint.DatabaseName}.{targetUser}] exists, granting [readWrite] role to database [{mongoEndpoint.DatabaseName}]");

                command = new BsonDocument() { { "grantRolesToUser", targetUser },
                { "roles", new BsonArray() { new BsonDocument() { { "role", "readWrite" }, { "db", mongoEndpoint.DatabaseName } } } } };
            }

            if (!loginEndpoint.GetDatabase(mongoEndpoint.DatabaseName).RunCommand<BsonDocument>(command).CheckSuccess())
                throw new ProcessFatalException($"Error executing command for [{mongoEndpoint.DatabaseName}.{targetUser}]");
        }

    }
}
