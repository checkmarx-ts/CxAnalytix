using log4net;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;

namespace CxAnalytix.Configuration
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

			if (!File.Exists(map.ExeConfigFilename))
				throw new FileNotFoundException("Configuration file missing.", map.ExeConfigFilename);

			_cfgManager = ConfigurationManager.OpenMappedExeConfiguration(map, ConfigurationUserLevel.None);

			EncryptSensitiveSections();

			Credentials = GetConfig<CxCredentials>(CxCredentials.SECTION_NAME);
			Connection = GetConfig<CxConnection>(CxConnection.SECTION_NAME);
			Service = GetConfig<CxAnalyticsService>(CxAnalyticsService.SECTION_NAME);
		}

		private static void EncryptSensitiveSections()
		{
			foreach (ConfigurationSection section in _cfgManager.Sections)
			{
				var attribs = section.GetType().GetCustomAttributes(typeof(SecureConfigSectionAttribute), true);

				if (attribs != null && attribs.Length > 0)
				{
					bool found = false;
					foreach (SecureConfigSectionAttribute attribInst in attribs)
					{
						if (attribInst.IsPropSet(section.GetType(), section))
						{
							found = true;
							break;
						}
					}

					if (!found)
						continue;
				}
				else
					continue;

				if (!section.SectionInformation.IsProtected)
				{
					section.SectionInformation.ProtectSection("DataProtectionConfigurationProvider");
					section.SectionInformation.ForceSave = true;
					section.SectionInformation.ForceDeclaration(true);
				}
			}

			try
			{
				_cfgManager.Save(ConfigurationSaveMode.Modified);
			}
			catch (Exception ex)
			{
				if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
					_log.Error("Exception trying to save application config.", ex);
				else
					_log.Warn("Sensitive configuration data can't be encrypted on this platform.  Use environment variables if possible.", ex);

			}
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
