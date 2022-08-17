using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;

namespace CxAnalytix.Utilities
{
    public class Reflection
    {

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
