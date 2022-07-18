using System;
using CxRestClient.MNO.Collections;

namespace CxRestClient.MNO.dto
{
    public class MNORuleDescriptor : PolicyRuleDescriptor
    {
        internal MNORuleDescriptor() { }

        public override int PolicyId { get; set; }
    }
}
