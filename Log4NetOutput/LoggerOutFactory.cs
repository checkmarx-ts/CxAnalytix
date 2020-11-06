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
            if (String.IsNullOrEmpty(recordType))
                throw new InvalidOperationException("Creating a logger with a blank record type is not valid.");

            return new LoggerOut(recordType);
        }

    }
}
