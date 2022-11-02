using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http.Headers;
using System.Reflection;

namespace CxAnalytix.Utilities
{
    public static class Reflection
    {

        public class UserAgentComponents
        {
            public UserAgentComponents()
            {
                CompanyName = "Checkmarx";
                ProductName = "CxAnalytix";
                ProductVersion = "0.0.0";
            }


            public String CompanyName { get; internal set; }
            public String ProductName { get; internal set; }
            public String ProductVersion { get; internal set; }
        }

        public static UserAgentComponents GetUserAgentName()
        {
            var assembly = System.Reflection.Assembly.GetEntryAssembly();

            var result = new UserAgentComponents();

            if (assembly != null)
            {
                var companyAttrib = assembly.CustomAttributes.FirstOrDefault((x) => x.AttributeType == typeof(System.Reflection.AssemblyCompanyAttribute));
                if (companyAttrib != null)
                    result.CompanyName = companyAttrib.ConstructorArguments[0].ToString().Replace("\"", "");

                var productAttrib = assembly.CustomAttributes.FirstOrDefault((x) => x.AttributeType == typeof(System.Reflection.AssemblyProductAttribute));
                if (productAttrib != null)
                    result.ProductName = productAttrib.ConstructorArguments[0].ToString().Replace("\"", "");

                var versionAttrib = assembly.CustomAttributes.FirstOrDefault((x) => x.AttributeType == typeof(System.Reflection.AssemblyInformationalVersionAttribute));
                if (versionAttrib != null)
                    result.ProductVersion = versionAttrib.ConstructorArguments[0].ToString().Replace("\"", "");
            }

            return result;
        }

        public static Assembly[] GetOutputAssemblies()
        {
            string[] runtimeAssemblies = Directory.GetFiles(Path.GetDirectoryName(Assembly.GetEntryAssembly().Location), "*.dll");
            List<Assembly> retVal = new List<Assembly>();

            foreach (var path in runtimeAssemblies)
            {
                try
                {
                    var asm = Assembly.LoadFrom(path);
                    retVal.Add(asm);
                }
                catch(FileLoadException)
                {
                    continue;
                }
                catch (BadImageFormatException)
                {
                    continue;
                }
            }

            return retVal.ToArray();
        }

    }
}
