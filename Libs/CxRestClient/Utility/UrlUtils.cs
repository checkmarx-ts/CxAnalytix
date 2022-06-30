using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CxRestClient.Utility
{
    public class UrlUtils
    {
        public static String MakeUrl(String url, String suffix)
        {
            if (url.EndsWith('/'))
                return url + suffix;
            else
                return url + '/' + suffix;

        }

        public static String MakeQueryString(Dictionary<String, String> query)
        {
            LinkedList<String> p = new LinkedList<string>();

            foreach (String k in query.Keys)
                p.AddLast(String.Format("{0}={1}", k, query[k]));

            return String.Join('&', p);
        }

        public static String MakeUrl(String url, String suffix, Dictionary<String, String> query)
        => MakeUrl(url, suffix) + ((query.Count > 0) ? ("?" + MakeQueryString(query)) : (""));


    }
}
