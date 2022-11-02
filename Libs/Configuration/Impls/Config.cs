using Autofac;
using CxAnalytix.Configuration.Utils;
using CxAnalytix.Exceptions;
using log4net;
using System;
using System.Configuration;
using System.IO;


namespace CxAnalytix.Configuration.Impls
{
    public class Config
    {
        private static System.Configuration.Configuration _cfgManager;
        private static ILog _log = LogManager.GetLogger(typeof (Config) );
		private static readonly String CONFIG_FILE_NAME = "cxanalytix.config";

		private static IContainer _configDI;


		static Config()
		{
			ExeConfigurationFileMap map = new ExeConfigurationFileMap();
			map.ExeConfigFilename = ConfigPathResolver.ResolveConfigFilePath(CONFIG_FILE_NAME);
			_log.DebugFormat("Loading configuration from [{0}]", map.ExeConfigFilename);

			if (!File.Exists(map.ExeConfigFilename))
				throw new FileNotFoundException("Configuration file missing.", map.ExeConfigFilename);

			if (!ConfigSectionValidator.IsValid(map.ExeConfigFilename))
				throw new UnrecoverableOperationException("The configuration is not valid.");

			_cfgManager = ConfigurationManager.OpenMappedExeConfiguration(map, ConfigurationUserLevel.None);

			if (OperatingSystem.IsWindows())
				EncryptSensitiveSections();
			else
				_log.Warn("This platform does not support encrypting credentials in the configuration file.  Your credentials may be stored in plain text.");

			var builder = new ContainerBuilder();

			foreach(var sec in _cfgManager.Sections)
            {
				builder.RegisterInstance(sec).As(sec.GetType()).ExternallyOwned();
            }
			_configDI = builder.Build();

		}

		public static T GetConfig<T>()
        {
			return _configDI.Resolve<T>();
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

            _cfgManager.Save(ConfigurationSaveMode.Modified);
		}

        
    }
}
