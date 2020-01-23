using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.Extensions.Configuration;
using Mono.Options;


namespace CxAPI_Core
{
    class Configuration
    {
        public static IConfigurationRoot _configuration;
        public static string[] _keys;


        public static IConfigurationRoot configuration(string[] args)
        {
         
            IConfigurationBuilder builder = new ConfigurationBuilder()
            .SetBasePath(System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location))
            .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
            .AddCommandLine(args);
            _keys = args;

            _configuration = builder.Build();
           

            return _configuration;
        }
        public static IConfigurationRoot configuration()
        {

            IConfigurationBuilder builder = new ConfigurationBuilder()
            .SetBasePath(System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location))
            .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true);
            _configuration = builder.Build();
            return _configuration;
        }

        public static settingClass get_settings()
        {
            settingClass _settings = new settingClass();
            _configuration.GetSection("CxRest").Bind(_settings);
            return _settings;
        }
            
            public static resultClass mono_command_args()
        {
            resultClass token = new resultClass();

            var p = new OptionSet() {
                { "t|get_token", "Fetch the bearer token from the CxSAST service",
                  v => token.api_action = api_action.getToken },
                { "c|store_credentials", "Store username and credential in an encrypted file",
                  v => token.api_action = api_action.storeCredentials },
                { "s|scan_results", "Get scan results, filtered by time and project",
                  v => token.api_action = api_action.scanResults },
                { "rn|report_name=", "Select desired report",
                  v => token.report_name= v },
                { "pn|project_name=", "Filter with project name, Will return project if any portion of the project name is a match",
                  v => token.project_name = v },
                { "pi|pipe", "Do not write to file but pipe output to stdout. Useful when using other API's",
                  v => token.pipe = true },
                { "path|file_path=", "Override file path in configuration",
                  v => token.file_path = v },
                { "file|file_name=", "Override file name in configuration",
                  v => token.file_name = v },
                { "sr|save_result=", "Enable saving of results as XML",
                  v => token.save_result = v },
                { "srp|save_result_path=", "Override save result path in configuration",
                  v => token.save_result_path = v },
                { "u|user_name=", "The username to use to retreive the token (REST) or session (SOAP)",
                  v => token.user_name = v },
                { "p|password=", "The password needed to retreive the token (REST) or session (SOAP)",
                  v => token.credential = v },
                { "st|start_time=", "Last scan start time",
                  v => token.start_time = DateTime.Parse(v) },
                { "et|end_time=", "Last scan end time",
                  v => token.end_time = DateTime.Parse(v) },
                { "v|verbose=", "Change degrees of debugging info",
                  v => token.verbosity = Convert.ToInt32(v) },
                { "d|debug", "Output debugging info ",
                  v => token.debug = true },
                { "?|h|help",  "show you your options",
                  v => token.api_action = api_action.help},
            };
            settingClass _settings = get_settings();
            token.save_result_filename = String.IsNullOrEmpty(token.save_result_filename) ? _settings.CxResultFileName : token.save_result_filename;
            token.save_result_path = String.IsNullOrEmpty(token.save_result_path) ? _settings.CxResultFilePath : token.save_result_path;
            token.file_name = String.IsNullOrEmpty(token.file_name) ? _settings.CxDefaultFileName : token.file_name;
            token.file_path = String.IsNullOrEmpty(token.file_path) ? _settings.CxFilePath : token.file_path;

            List<string> extra;
            try
            {
                extra = p.Parse(_keys);
            }
 
           // if (String.IsNullOrEmpty(token.file_name) ? _configuration
            catch (OptionException e)
            {
                Console.Write("CxApi_Core: ");
                Console.WriteLine(e.Message);
                Console.WriteLine("Try CxApi --help' for more information.");
                token.status = -1;
            }

            if (token.api_action == api_action.help)
            {
                ShowHelp(p);
            }
            return token;
        }


        private static void ShowHelp(OptionSet p)
        {
            Console.WriteLine("Usage: CxApi action arguments");
            Console.WriteLine();
            Console.WriteLine("Options:");
            p.WriteOptionDescriptions(Console.Out);
        }
    }


}


