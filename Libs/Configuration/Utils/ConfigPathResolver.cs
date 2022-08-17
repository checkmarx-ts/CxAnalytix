using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CxAnalytix.Configuration.Utils
{
    public class ConfigPathResolver
    {
        private static readonly String CONFIG_PATH_VARIABLE = "CXANALYTIX_CONFIG_PATH";
        private static readonly String DEFAULT_FOLDER_NAME = "cxanalytix";
        private static readonly String DEFAULT_LINUX_PATH = $"/etc/{DEFAULT_FOLDER_NAME}";

        public static String ResolveConfigFilePath(String fileName)
        {
            String cwd = Path.Combine(Directory.GetCurrentDirectory(), fileName);

            if (File.Exists(cwd))
                return cwd;

            if (Environment.GetEnvironmentVariables()[CONFIG_PATH_VARIABLE] != null)
            {
                String env = Path.Combine(Environment.GetEnvironmentVariables()[CONFIG_PATH_VARIABLE] as String, fileName);
                if (File.Exists(env))
                    return env;
            }

            String os = String.Empty;

            if (OperatingSystem.IsWindows())
                os = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), DEFAULT_FOLDER_NAME, fileName);
            else
                os = Path.Combine(DEFAULT_LINUX_PATH, fileName);

            if (File.Exists(os))
                return os;

            throw new FileNotFoundException($"Configuration file {fileName} could not be located.");
        }
    }
}
