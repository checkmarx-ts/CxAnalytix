using CxAnalytix.Interfaces.Outputs;

namespace CxAnalytix.Out.MongoDBOutput
{
    internal interface ISchema : IRecordRef
    {
        bool VerifyOrCreateSchema();
    }
}
