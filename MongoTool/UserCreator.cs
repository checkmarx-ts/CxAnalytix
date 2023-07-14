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

        public static void CreateOrUpdateUser(MongoUrl mongoEndpoint, String loginDBName)
        {
            try
            {
                CreateOrUpdateUser(mongoEndpoint, loginDBName, mongoEndpoint.Username, mongoEndpoint.Password);
            }
            catch (MongoCommandException ex)
            {
                _log.Warn($"Failed to set roles for database [{mongoEndpoint.DatabaseName}] user [{mongoEndpoint.Username}] Error [{ex.Message}]");
            }
        }

        private static void CreateOrUpdateUser(IMongoDatabase inThisDB, String targetDBName, String targetUser, String targetPassword)
        {
            var getUsersCommand = new BsonDocument() { { "usersInfo", 1 } };
            var usersDB = inThisDB.RunCommand<BsonDocument>(getUsersCommand);

            var foundUser = usersDB.GetElement("users").Value.AsBsonArray.SingleOrDefault((bval) => bval["user"].AsString.ToLower().CompareTo(targetUser.ToLower()) == 0);

            BsonDocument command = null;
            if (foundUser == null || foundUser.IsBsonNull)
            {
                _log.Info($"Creating user [{inThisDB.DatabaseNamespace}.{targetUser}] with [readWrite] role for database [{targetDBName}]");

                command = new BsonDocument() { { "createUser", targetUser }, { "pwd", targetPassword},
                { "roles", new BsonArray() { new BsonDocument() { { "role", "readWrite" }, { "db", targetDBName } } } } };

            }
            else
            {
                _log.Info($"User [{inThisDB.DatabaseNamespace}.{targetUser}] exists, granting [readWrite] role for database [{targetDBName}]");

                command = new BsonDocument() { { "grantRolesToUser", targetUser },
                { "roles", new BsonArray() { new BsonDocument() { { "role", "readWrite" }, { "db", targetDBName } } } } };
            }

            if (!inThisDB.RunCommand<BsonDocument>(command).CheckSuccess())
                throw new ProcessFatalException($"Error executing command for [{inThisDB.DatabaseNamespace}.{targetUser}]");
        }

        public static void CreateOrUpdateUser(MongoUrl mongoEndpoint, String loginDBName, String targetUser, String targetPassword)
        {
            var adminDB = mongoEndpoint.ChangeToDB(loginDBName).GetDatabase();
            var targetDB = adminDB.Client.GetDatabase(mongoEndpoint.DatabaseName);

            CreateOrUpdateUser(adminDB, mongoEndpoint.DatabaseName, targetUser, targetPassword);
            CreateOrUpdateUser(targetDB, mongoEndpoint.DatabaseName, targetUser, targetPassword);
        }

    }
}
