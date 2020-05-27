using CxAnalytix.TransformLogic;
using System;
using System.Collections.Generic;
using System.Text;

namespace CxAnalytix.Out.MongoDBOutput
{
    public sealed class MongoDBOutFactory : IOutputFactory
    {
        public IOutput newInstance(string recordType)
        {
            /*
             * Configs:
             * 
             * connection string (mongodb://localhost:27017)
             * database name
             * 
             * 
             * the collections are based on the record name set up in the service.  
             * 
             * might need a tool to create the schema so that the indexes are created properly.
             * should be able to override the collection names, then the collection names are set in the service config
             * 
             * need to refactor to allow String, Object dictionary.
             * 
            */

            return new MongoDBOut(recordType);
        }
    }
}
