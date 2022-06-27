using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SDK
{
    public sealed class Registrar
    {
        public static readonly Registrar ModuleRegistry;

        private HashSet<string> _mods = new HashSet<string>();    

        static Registrar()
        {
            ModuleRegistry = new Registrar();
        }

        public void RegisterModule(String moduleName)
        {
            _mods.Add(moduleName);
        }

        public IEnumerable<String> ModuleNames => _mods;

    }
}
