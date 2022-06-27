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
        protected String StorageFilename { get; private set; }

        public TransformerModule(string moduleName, Type moduleImplType, String storageFileName) : base(moduleName, moduleImplType)
        {
            StorageFilename = storageFileName;
        }

        public abstract void DoTransform(CancellationToken token);
    }
}
