using log4net;
using System;
using System.Collections.Generic;
using System.Text;

namespace CxRestClient
{
	public abstract class CxRestContextBase
	{

		private static ILog _log = LogManager.GetLogger(typeof(CxRestContextBase));

		#region Public/Private Properties

		public bool ValidateSSL { get; internal set; }
		public String Url { get; internal set; }
		public TimeSpan Timeout { get; internal set; }
		public int RetryLoop { get; internal set; }
		#endregion

		public abstract void Reauthenticate();



        #region Static utility methods
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

        #endregion

    }
}
