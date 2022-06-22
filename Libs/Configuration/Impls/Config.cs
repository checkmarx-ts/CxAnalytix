using CxAnalytix.Configuration.Utils;
using log4net;
using System;
using System.Configuration;
using System.IO;
using System.Reflection;
using System.Composition.Hosting;
using System.Composition;

namespace CxAnalytix.Configuration.Impls
{
    public class Config
    {
        private static System.Configuration.Configuration _cfgManager;
        private static ILog _log = LogManager.GetLogger(typeof (Config) );
		private static readonly String CONFIG_FILE_NAME = "cxanalytix.config";
		private static readonly String CONFIG_PATH_VARIABLE = "CXANALYTIX_CONFIG_PATH";
		private static readonly String DEFAULT_FOLDER_NAME = "cxanalytix";
		private static readonly String DEFAULT_LINUX_PATH = $"/etc/{DEFAULT_FOLDER_NAME}";


		static Config()
		{
			ExeConfigurationFileMap map = new ExeConfigurationFileMap();
			map.ExeConfigFilename = FindConfigFilePath();
			_log.DebugFormat("Loading configuration from [{0}]", map.ExeConfigFilename);

			if (!File.Exists(map.ExeConfigFilename))
				throw new FileNotFoundException("Configuration file missing.", map.ExeConfigFilename);

			_cfgManager = ConfigurationManager.OpenMappedExeConfiguration(map, ConfigurationUserLevel.None);

			if (OperatingSystem.IsWindows())
				EncryptSensitiveSections();
			else
				_log.Warn("This platform does not support encrypting credentials in the configuration file.  Your credentials may be stored in plain text.");
		}

		internal static System.Configuration.ConfigurationSectionCollection Sections => _cfgManager.Sections;

		private static ContainerConfiguration ContConfig(Assembly execAssembly)
        {
			return new ContainerConfiguration().WithAssembly(execAssembly);
		}

		public static void InjectMyConfigs<T>(T inst, Assembly execAssembly)
        {
			using (var container = ContConfig(execAssembly).CreateContainer())
				container.SatisfyImports(inst);
		}

		public static T GetConfig<T>(Assembly execAssembly)
        {
			using (var container = ContConfig(execAssembly).CreateContainer())
				return container.GetExport<T>();
		}
		public static T GetConfig<T>()
		{
			using (var container = ContConfig(Assembly.GetExecutingAssembly() ).CreateContainer())
				return container.GetExport<T>();
		}

		public static void InjectServiceConfigs<T>(T inst)
        {
			InjectMyConfigs(inst, Assembly.GetExecutingAssembly());

		}


		private static String FindConfigFilePath()
        {
			String cwd = Path.Combine(Directory.GetCurrentDirectory(), CONFIG_FILE_NAME);

            if (File.Exists(cwd))
                return cwd;

            if (Environment.GetEnvironmentVariables()[CONFIG_PATH_VARIABLE] != null)
			{
				String env = Path.Combine(Environment.GetEnvironmentVariables()[CONFIG_PATH_VARIABLE] as String, CONFIG_FILE_NAME);
				if (File.Exists(env))
					return env;
			}

			String os = String.Empty;

            if (OperatingSystem.IsWindows())
                os = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), DEFAULT_FOLDER_NAME, CONFIG_FILE_NAME);
            else
                os = Path.Combine(DEFAULT_LINUX_PATH, CONFIG_FILE_NAME);

            if (File.Exists(os))
				return os;

			throw new FileNotFoundException($"Configuration file {CONFIG_FILE_NAME} could not be located.");
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
