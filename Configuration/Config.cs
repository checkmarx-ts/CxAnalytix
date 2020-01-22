using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.Text;

namespace CxAnalytics.Configuration
{
    public class Config
    {
        private static System.Configuration.Configuration _cfgManager;

        static Config()
        {
            ExeConfigurationFileMap map = new ExeConfigurationFileMap();
            map.ExeConfigFilename = Process.GetCurrentProcess().MainModule.ModuleName + ".config";
            _cfgManager = ConfigurationManager.OpenMappedExeConfiguration(map, ConfigurationUserLevel.None);

            Credentials = _cfgManager.Sections[CxCredentials.SECTION_NAME] as CxCredentials;
            if (!Credentials.SectionInformation.IsProtected)
            {
                Credentials.SectionInformation.ProtectSection("DataProtectionConfigurationProvider");
                Credentials.SectionInformation.ForceSave = true;
                Credentials.SectionInformation.ForceDeclaration(true);

                _cfgManager.Save(ConfigurationSaveMode.Modified);
            }

            Connection = _cfgManager.Sections[CxConnection.SECTION_NAME] as CxConnection;
            Service = _cfgManager.Sections[CxAnalyticsService.SECTION_NAME] as CxAnalyticsService;
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


        public static T GetConfig<T>(String sectionName) where T : ConfigurationSection
        {
            return _cfgManager.Sections[sectionName] as T;
        }
        
    }
}
