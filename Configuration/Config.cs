using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.Text;

namespace CxAnalytics.Configuration
{
    public class Config
    {

        static Config()
        {
            ExeConfigurationFileMap map = new ExeConfigurationFileMap();
            map.ExeConfigFilename = Process.GetCurrentProcess().MainModule.ModuleName + ".config";
            var mgr = ConfigurationManager.OpenMappedExeConfiguration(map, ConfigurationUserLevel.None);

            Credentials = mgr.Sections[CxCredentials.SECTION_NAME] as CxCredentials;
            if (!Credentials.SectionInformation.IsProtected)
            {
                Credentials.SectionInformation.ProtectSection("DataProtectionConfigurationProvider");
                Credentials.SectionInformation.ForceSave = true;
                Credentials.SectionInformation.ForceDeclaration(true);

                mgr.Save(ConfigurationSaveMode.Modified);
            }

            Connection = mgr.Sections[CxConnection.SECTION_NAME] as CxConnection;
            Service = mgr.Sections[CxAnalyticsService.SECTION_NAME] as CxAnalyticsService;
        }


        public static CxCredentials Credentials
        {
            get;
            private set;
        }

        public static CxConnection Connection
        {
            get;
            private set;
        }

        public static CxAnalyticsService Service
        {
            get;
            private set;
        }
        
    }
}
