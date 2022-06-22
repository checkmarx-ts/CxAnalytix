using CxAnalytix.Configuration.Contracts;
using CxAnalytix.Configuration.Impls;
using System;
using System.Collections.Generic;
using System.Composition;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CxAnalytix.Configuration.Utils
{
    public abstract class MEFableConfigurationSection : ConfigurationSection
    {
        private ConfigurationSection _inst;
        protected T Instance<T>() where T : ConfigurationSection
        {
                return _inst as T;
        }

        public MEFableConfigurationSection()
        {
            // Constructor used when loading system configuration.
            _inst = this;
        }

        public MEFableConfigurationSection(IConfigSectionResolver resolver)
        {
            // Constructor used when MEF loads the instance, it will redirect
            // to the instance loaded by the system configuration.
            _inst = resolver.GetSectionInstance(this.GetType());
        }


    }
}
