using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CxRestClient.Utility
{
    public static class UrlUtils
    {
        public static String MakeUrl(String url, params String[] suffixes)
        {
            StringBuilder result = new(url.TrimEnd('/'));

            foreach (var suffix in suffixes)
                result.Append("/").Append(suffix.TrimStart('/'));

            return result.ToString();
        }

        public static String MakeQueryString(IDictionary<String, String> query)
        {
            LinkedList<String> p = new LinkedList<string>();

            foreach (String k in query.Keys)
                p.AddLast(String.Format("{0}={1}", k, query[k]));

            return String.Join('&', p);
        }

        public static String QueryMarkerValue(String originalUrl)
        {
            if (originalUrl.Contains('?'))
                return "&";
            else
                return "?";
        }

        public static String MakeUrl(String url, String suffix, IDictionary<String, String> query)
        => MakeUrl(url, suffix) + ((query.Count > 0) ? (QueryMarkerValue(url+suffix) + MakeQueryString(query)) : (""));

        public static String MakeUrl(String url, Dictionary<String, String> query)
        => url + ((query.Count > 0) ? (QueryMarkerValue(url) + MakeQueryString(query)) : (""));

    }
}
