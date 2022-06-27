using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SDK.Modules
{
    public sealed class Registrar
    {
        public static readonly Registrar ModuleRegistry;

        private Dictionary<Type, HashSet<string>> _registry = new Dictionary<Type, HashSet<string>>();

        static Registrar()
        {
            ModuleRegistry = new Registrar();
        }

        public void RegisterModule<T>(String moduleName)
        {
            if (!_registry.ContainsKey(typeof(T)))
                _registry[typeof(T)] = new HashSet<string>();

            _registry[typeof(T)].Add(moduleName);
        }

        public IEnumerable<String> GetModuleNames<T>() => _registry[typeof(T)];

    }
}
