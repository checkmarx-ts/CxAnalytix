using System;
using System.Collections.Generic;
using System.Configuration;
using System.Text;
using System.Xml;
using System.Xml.Serialization;

namespace CxAnalytix.Configuration
{
    public sealed class CxCredentials : ConfigurationSection
    {
        internal CxCredentials ()
        {

        }

        public static readonly String SECTION_NAME = "CxCredentials";

        [ConfigurationProperty("Username", IsRequired = false)]
        public String Username {
            get => (String)this["Username"];
            set { this["Username"] = value; }
        }

        [ConfigurationProperty("Password", IsRequired = false)]
        public String Password
        {
            get => (String)this["Password"];
            set { this["Password"] = value; }
        }

        [ConfigurationProperty("Token", IsRequired = false)]
        public String Token 
        {
            get => (String) this["Token"];
            set { this["Token"] = value; }
        }


        private void copy(CxCredentials fromInst)
        {
            Username = fromInst.Username;
            Password = fromInst.Password;
            Token = fromInst.Token;
        }
 
    }
}
