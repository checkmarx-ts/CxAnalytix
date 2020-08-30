using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using log4net.Repository;

namespace CxRestClient.IO
{
    public class NetworkTraceLogAttribute : log4net.Config.ConfiguratorAttribute
    {

        public NetworkTraceLogAttribute () : base (int.MaxValue)
        {

        }

        public override void Configure(Assembly sourceAssembly, ILoggerRepository targetRepository)
        {
            targetRepository.LevelMap.Add(NetworkTraceExtension._netTraceLevel);
        }
    }
}
