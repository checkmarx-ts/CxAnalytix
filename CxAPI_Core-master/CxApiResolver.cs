using System;
using System.Collections.Generic;
using System.Text;
using CxAPI_Core;

namespace CxApiResolver
{
    public partial class CxWSResolverSoapClient : System.ServiceModel.ClientBase<CxApiResolver.CxWSResolverSoap>, CxApiResolver.CxWSResolverSoap
    {
        static partial void ConfigureEndpoint(System.ServiceModel.Description.ServiceEndpoint serviceEndpoint, System.ServiceModel.Description.ClientCredentials clientCredentials)
        {
            secure secure = new secure();
            settingClass settings = secure.get_settings();
            if (settings.debug == "true")
            {
                Console.WriteLine("CxApiResolver called: {0}", settings.CxAPIResolver);
            }

            serviceEndpoint.Address =
                new System.ServiceModel.EndpointAddress(new System.Uri(settings.CxAPIResolver),
                new System.ServiceModel.DnsEndpointIdentity(""));
        }
    }
}
