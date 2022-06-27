using SDK.Modules.Transformer.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CxRestClient.MNO.dto
{
    public class MNORuleDescriptor : PolicyRuleDescriptor
    {
        internal MNORuleDescriptor() { }

        public override int PolicyId { get; set; }
    }
}
