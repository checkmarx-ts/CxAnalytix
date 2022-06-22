using CxAnalytix.Out.MongoDBOutput.Config.Impl;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CxAnalytix.Out.MongoDBOutput.Config.Contracts
{
    public interface IMongoOutConfig
    {
        ShardKeySpecConfig ShardKeys { get; }
    }
}
