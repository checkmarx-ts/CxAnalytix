using CxAnalytix.Configuration.Impls;
using System;
using System.Configuration;

namespace CxAnalytix.XForm.ScaTransformer.Config
{
    public class CxScaConnection : CxConnection
    {
        [ConfigurationProperty("LoginURL", IsRequired = true)]
        public String LoginURL
        {
            get => (String)this["LoginURL"];
            set { this["LoginURL"] = value; }
        }

    }
}
