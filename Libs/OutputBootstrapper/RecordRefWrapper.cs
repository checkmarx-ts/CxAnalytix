using CxAnalytix.Interfaces.Outputs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OutputBootstrapper
{
    internal class RecordRefWrapper : IRecordRef
    {
        private IRecordRef _wrapped;

        public RecordRefWrapper() 
        {
            Suppressed = true;
            _wrapped = null;
        }

        public RecordRefWrapper(IRecordRef original)
        {
            Suppressed = false;
            _wrapped = original;
        }

        internal IRecordRef OriginalRef => _wrapped;

        public bool Suppressed { get; private set; }

        public string RecordName => (_wrapped == null) ? ("Suppressed") : (_wrapped.RecordName);
    }
}
