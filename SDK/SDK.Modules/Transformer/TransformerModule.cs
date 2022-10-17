using CxAnalytix.Interfaces.Transform;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SDK.Modules.Transformer
{
    public abstract class TransformerModule : Common.CxAnalytixModule<ITransformer>, ITransformer
    {
        protected String StateStorageFilename { get; private set; }

        public abstract string DisplayName { get; }

        public TransformerModule(string moduleName, Type moduleImplType, String stateStorageFileName) : base(moduleName, moduleImplType)
        {
            StateStorageFilename = stateStorageFileName;
        }

        public abstract void DoTransform(CancellationToken token);

        public abstract void Dispose();
    }
}
