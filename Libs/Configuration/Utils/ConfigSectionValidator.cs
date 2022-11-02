using CxAnalytix.Configuration.Properties;
using log4net;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text;
using System.Xml;
using System.Xml.XPath;

[assembly: InternalsVisibleTo("Configuration_Tests")]
namespace CxAnalytix.Configuration.Utils
{
    internal class ConfigSectionValidator
    {
        private static ILog _log = LogManager.GetLogger(typeof(ConfigSectionValidator));

        internal class Directives
        {

            [JsonProperty(PropertyName = "required_sections")]
            public Dictionary<String, String> RequiredSections { get; internal set; }

            [JsonProperty(PropertyName = "removable_sections")]
            public List<String> RemoveSections { get; internal set; }
        }

        internal Directives SectionDirectives { get; set; } = null;
        internal String FilePath { get; set; }

        internal XmlDocument ConfigDocument { get; set; } = new();

        internal Dictionary<String, XPathNavigator> ConfigNodeIndex { get; set; }


        internal void ReadXML()
        {
            ConfigNodeIndex = new();

            ConfigDocument.Load(FilePath);
            var nav = ConfigDocument.CreateNavigator();
            var sections = nav.Evaluate("/configuration/configSections/section") as XPathNodeIterator;

            foreach (XPathNavigator section in sections)
                ConfigNodeIndex.Add(section.GetAttribute("name", ""), section);
        }

        internal ConfigSectionValidator(String pathToConfigXML)
        {
            FilePath = pathToConfigXML;

            using (var resource = new MemoryStream(Resources.config_sections))
            using (var reader = new StreamReader(resource))
            using (var jsonReader = new JsonTextReader(reader))
                SectionDirectives = (Directives)new JsonSerializer().Deserialize(jsonReader, typeof(Directives));

            ReadXML();
        }


        public static bool IsValid(String pathToConfigXML)
        {
            bool isValid = true;

            var csu = new ConfigSectionValidator(pathToConfigXML);

            foreach (var removeSection in csu.SectionDirectives.RemoveSections)
                if (csu.ConfigNodeIndex.ContainsKey(removeSection))
                {
                    isValid = false;
                    var sectionNode = csu.ConfigNodeIndex[removeSection];
                    _log.Error($"Configuration is invalid! Please remove XML element [{sectionNode.OuterXml}] from configuration file [{pathToConfigXML}]");
                }

            foreach (var requiredSection in csu.SectionDirectives.RequiredSections)
                if (!csu.ConfigNodeIndex.ContainsKey(requiredSection.Key))
                {
                    isValid = false;
                    _log.Error("Configuration is invalid! Please add " + 
                        $"XML element [<section name=\"{requiredSection.Key}\" type=\"{requiredSection.Value}\" />] to " +
                        $"the \"configSection\" element in configuration file [{pathToConfigXML}]");
                }

            return isValid;
        }


    }
}
