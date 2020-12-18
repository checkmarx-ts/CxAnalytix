using CxAnalytix.Interfaces.Outputs;

namespace CxAnalytix.Out.MongoDBOutput
{
    internal interface ISchema : IOutput
    {
        bool VerifyOrCreateSchema();
    }
}
