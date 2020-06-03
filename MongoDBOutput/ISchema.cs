using CxAnalytix.TransformLogic;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Text;

namespace CxAnalytix.Out.MongoDBOutput
{
    internal interface ISchema : IOutput
    {
        bool VerifyOrCreateSchema();
    }
}
