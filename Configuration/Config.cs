using log4net;
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
        private static ILog _log = LogManager.GetLogger(typeof (Config) );

        static Config()
        {
            ExeConfigurationFileMap map = new ExeConfigurationFileMap();
            var process = Process.GetCurrentProcess();
            map.ExeConfigFilename = process.MainModule.ModuleName + ".config";
            _log.DebugFormat("Loading configuration from [{0}]", map.ExeConfigFilename);

            _cfgManager = ConfigurationManager.OpenMappedExeConfiguration(map, ConfigurationUserLevel.None);

            Credentials = _cfgManager.Sections[CxCredentials.SECTION_NAME] as CxCredentials;
            if (Credentials != null && !Credentials.SectionInformation.IsProtected)
            {
                Credentials.SectionInformation.ProtectSection("DataProtectionConfigurationProvider");
                Credentials.SectionInformation.ForceSave = true;
                Credentials.SectionInformation.ForceDeclaration(true);

                try
                {
                    _cfgManager.Save(ConfigurationSaveMode.Modified);
                }
                catch (Exception ex)
                {
                    _log.Error("Exception trying to save application config.", ex);
                }
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
