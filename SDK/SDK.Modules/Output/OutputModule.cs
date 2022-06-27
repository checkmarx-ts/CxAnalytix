using Autofac;
using CxAnalytix.Interfaces.Outputs;

namespace SDK.Modules.Output
{
    public abstract class OutputModule : Common.CxAnalytixModule<IOutputFactory>, IOutputFactory
    {

        public OutputModule(String moduleName, Type moduleImplType) : base(moduleName, moduleImplType)
        {
        }

        public abstract IRecordRef RegisterRecord(string recordName);
        public abstract IOutputTransaction StartTransaction();
    }
}