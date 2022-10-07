using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CxRestClient.CXONE.Common
{
    [JsonObject(MemberSerialization.OptIn)]
    public abstract class TotaledArray
    {
        [JsonProperty(PropertyName = "totalCount")]
        public UInt32 Total { get; internal set; }

    }
}
