using Autofac;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SDK.Modules.Common
{
    public abstract class CxAnalytixModule<T> : Module where T : notnull
    {
        private String _name;
        private Type _type;

        public CxAnalytixModule(String moduleName, Type moduleImplType)
        {
            _name = moduleName;
            _type = moduleImplType;
        }

        protected override void Load(ContainerBuilder builder)
        {
            base.Load(builder);
            builder.RegisterType(_type).Named<T>(_name.ToLower()).Named<T>(_name);
            Registrar.ModuleRegistry.RegisterModule<T>(_name);
        }
    }
}
