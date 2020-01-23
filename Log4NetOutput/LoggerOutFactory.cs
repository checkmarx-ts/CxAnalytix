using CxAnalytics.TransformLogic;
using System;
using System.Collections.Generic;
using System.Text;

namespace CxAnalytics.Out.Log4NetOutput
{
    public sealed class LoggerOutFactory : IOutputFactory
    {
        public IOutput newInstance(String recordType)
        {
            return new LoggerOut(recordType);
        }

    }
}
