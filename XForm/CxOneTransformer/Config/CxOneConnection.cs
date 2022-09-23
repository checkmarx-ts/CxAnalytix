using CxAnalytix.Configuration.Impls;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CxAnalytix.XForm.CxOneTransformer.Config
{
    public class CxOneConnection : CxConnection
    {
        [ConfigurationProperty("IAMURL", IsRequired = true)]
        public String IamUrl
        {
            get => (String)this["IAMURL"];
            set { this["IAMURL"] = value; }
        }

    }
}
