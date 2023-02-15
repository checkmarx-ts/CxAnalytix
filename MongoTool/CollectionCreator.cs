using CxAnalytix.Configuration.Impls;
using CxAnalytix.Configuration.Utils;
using CxAnalytix.Exceptions;
using CxAnalytix.Extensions;
using log4net;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Drawing.Printing;
using System.Linq;
using System.Reflection;

namespace CxAnalytix.MongoTool
{
    internal static class CollectionCreator
    {
        private static ILog _log = LogManager.GetLogger(typeof(CollectionCreator));
        
        public static void CreateCollections(MongoUrl mongoEndpoint)
        {
            var client = new MongoClient(mongoEndpoint);

            if (!client.ListDatabaseNames().ToList().Contains(mongoEndpoint.DatabaseName))
                _log.Warn($"Database {mongoEndpoint.DatabaseName} does not exist, it will be created.");

            var db = mongoEndpoint.GetDatabase();

            var existingCollections = new HashSet<String>(db.ListCollectionNames().ToEnumerable());

            var serviceConfig = Config.GetConfig<CxAnalytixService>();

            if (serviceConfig == null)
                throw new ProcessFatalException("Could not retrieve the CxAnalytix service configuration from the CxAnalytix configuration file.");


            typeof(CxAnalytixService).GetProperties().ActionForEach(p =>
            {
                if (p.GetCustomAttribute<RecordNameConfigAttribute>() != null)
                {
                    if (!existingCollections.Contains(p.GetValue(serviceConfig)))
                    {
                        _log.Info($"Creating collection for record type [{p.Name}] with the name [{p.GetValue(serviceConfig)}].");
                        db.CreateCollection(p.GetValue(serviceConfig) as String);
                    }
                    else
                        _log.Info($"Collection for record type [{p.Name}] named [{p.GetValue(serviceConfig)}] already exists.");
                }
            });
        }
    }
}
