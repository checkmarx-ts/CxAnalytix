using CxAnalytix.Out.Log4NetOutput.Config.Impl;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CxAnalytix.Out.Log4NetOutput.Config.Contracts
{
    public interface ILogOutputConfig
    {
        int DataRetentionDays { get; }
        String OutputRoot { get; }
        FileSpecElementCollection PurgeSpecs { get; }
    }
}
