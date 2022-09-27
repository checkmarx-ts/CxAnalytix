using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CxRestClient.CXONE.Common
{
    [JsonObject(MemberSerialization.OptIn)]
    public abstract class WrappedArray
    {
        [JsonProperty(PropertyName = "totalCount")]
        public UInt32 Total { get; internal set; }

        [JsonProperty(PropertyName = "filteredTotalCount")]
        public UInt32 FilteredTotal { get; internal set; }

    }
}
