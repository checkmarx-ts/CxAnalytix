using CxAnalytix.TransformLogic;
using System;
using System.Collections.Generic;
using System.Text;
using CxAnalytix.Interfaces.Outputs;

namespace CxAnalytix.Out.Log4NetOutput
{
    public sealed class LoggerOutFactory : IOutputFactory
    {
        public IOutput newInstance(String recordType)
        {
            return new LoggerOut(recordType);
        }

    }
}
