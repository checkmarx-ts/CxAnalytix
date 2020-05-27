using System;
using System.Collections.Generic;
using System.Text;

namespace CxRestClient
{
    public class RuleDescriptor
    {
        internal RuleDescriptor()
        { }

        public int RuleId { get; set; }
        public String Name { get; set; }
        public String Description { get; set; }
        public String ScanProduct { get; set; }
        public String RuleType { get; set; }
        public FormattedDateTime CreatedOn { get; set; }
    }
}
