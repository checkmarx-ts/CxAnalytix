using CxAnalytix.Configuration.Utils;
using log4net;
using System;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Linq;
using System.Reflection;

namespace CxAnalytix.Configuration.Impls
{
    internal class Config
    {
        private static System.Configuration.Configuration _cfgManager;
        private static ILog _log = LogManager.GetLogger(typeof (Config) );
		private static Boolean _boostrapped;

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
			_boostrapped = true;
		}


		internal static void AutoInit<T>(T dest) where T : ConfigurationSection
		{
			if (!_boostrapped)
				return;

			foreach (var section in _cfgManager.Sections)
				if (section.GetType() == typeof(T))
				{
					var classProps = section.GetType().FindMembers(System.Reflection.MemberTypes.Property,
						System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public, (f,x) =>
						{
							var propInfo = ((PropertyInfo)f);
							return propInfo.CanWrite && propInfo.CanRead && propInfo.DeclaringType == typeof(T);
						}, null);

					foreach (PropertyInfo pi in classProps)
					{
						var gm = pi.GetGetMethod(); 
						var sm = pi.GetSetMethod();

						object curVal = typeof(T).InvokeMember(gm.Name, BindingFlags.InvokeMethod, null, section, null);

						typeof(T).InvokeMember(sm.Name, BindingFlags.InvokeMethod, null, dest, new object[] { curVal });

					}
				}
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

        
    }
}
