using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CxAnalytix.Configuration.Impls
{
    public sealed class CxSASTAPIOverrides : ConfigurationElement
    {

        [ConfigurationProperty("Project", IsRequired = false, DefaultValue = false)]
        public bool Project
        {
            get => (bool)this["Project"];
            set => this["Project"] = value;
        }
    }
}
