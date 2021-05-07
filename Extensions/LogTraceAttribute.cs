using log4net.Repository;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace CxAnalytix.Extensions
{
	    public class LogTraceAttribute : log4net.Config.ConfiguratorAttribute
    {

        public LogTraceAttribute() : base(int.MaxValue)
        {

        }

        public override void Configure(Assembly sourceAssembly, ILoggerRepository targetRepository)
        {
            targetRepository.LevelMap.Add(LogTraceExtension._level);
        }
    }

}
