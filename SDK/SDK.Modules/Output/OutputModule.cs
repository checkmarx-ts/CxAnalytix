using Autofac;
using CxAnalytix.Interfaces.Outputs;

namespace SDK.Modules
{
    public abstract class OutputModule : Module, IOutputFactory
    {
        private String _name;
        private Type _type;

        public OutputModule(String moduleName, Type moduleImplType)
        {
            _name = moduleName;
            _type = moduleImplType;
        }

        public abstract IRecordRef RegisterRecord(string recordName);
        public abstract IOutputTransaction StartTransaction();

        protected override void Load(ContainerBuilder builder)
        {
            base.Load(builder);
            builder.RegisterType(_type).Named<IOutputFactory>(_name.ToLower()).Named<IOutputFactory>(_name);
            Registrar.ModuleRegistry.RegisterModule<IOutputFactory>(_name);
        }


    }
}