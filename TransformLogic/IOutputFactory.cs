using System;
using System.Collections.Generic;
using System.Text;

namespace CxAnalytics.TransformLogic
{
    public interface IOutputFactory
    {
        IOutput newInstance(String recordType);

    }
}
