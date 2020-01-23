using System;
using System.Collections.Generic;
using System.Text;

namespace CxAPI_Core.dto
{


    public class SourceSettingsLink
    {
        public string type { get; set; }
        public string rel { get; set; }
        public object uri { get; set; }
    }

    public class ProjectLink
    {
        public string rel { get; set; }
        public string uri { get; set; }
    }

    public class ProjectObject
    {
        public class SourceSettingsLink
        {
            public string type { get; set; }
            public string rel { get; set; }
            public string uri { get; set; }
        }
        public class ProjectLink
        {
            public string rel { get; set; }
            public string uri { get; set; }
        }
        public string id { get; set; }
        public string teamId { get; set; }
        public string name { get; set; }
        public string isPublic { get; set; }
        public SourceSettingsLink sourceSettingsLink { get; set; }
        public Link link { get; set; }
    }
}

