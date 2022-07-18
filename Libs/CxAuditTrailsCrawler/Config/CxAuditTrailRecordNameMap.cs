using System;
using System.Text;

namespace CxAnalytix.AuditTrails.Crawler.Config
{
	public class CxAuditTrailRecordNameMap : CxAuditTrailOpts<String>
	{

		private static String DefaultRecordName (String name)
		{
			StringBuilder sb = new StringBuilder("RECORD_");

			foreach (Char c in name)
			{
				if (Char.IsLetterOrDigit(c))
					sb.Append(c);
			}

			return sb.ToString ();
		}
	}
}
