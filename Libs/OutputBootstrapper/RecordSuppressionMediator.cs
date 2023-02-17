using CxAnalytix.Interfaces.Outputs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OutputBootstrapper
{
    internal class RecordSuppressionMediator : IOutputTransaction
    {
        private IOutputTransaction _wrapped;

        public RecordSuppressionMediator(IOutputTransaction wrapped) => _wrapped = wrapped;


        public string TransactionId => _wrapped.TransactionId;

        public bool Commit() => _wrapped.Commit();  

        public void Dispose() => _wrapped.Dispose();

        public void write(IRecordRef which, IDictionary<string, object> record)
        {
            var wrappedRef = which as RecordRefWrapper;

            if (!wrappedRef.Suppressed)
                _wrapped.write(wrappedRef.OriginalRef, record);

        }
    }
}
