using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CxAnalytix.Out.MongoDBOutput.Config.Contracts
{
    public interface IMongoConnectionConfig
    {
        string ConnectionString { get; }
    }
}
