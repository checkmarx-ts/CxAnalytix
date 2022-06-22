using CxAnalytix.AuditTrails.Crawler.Contracts;
using System;
using System.Composition;
using System.Text;

namespace CxAnalytix.AuditTrails.Crawler.Config
{
	[Export(typeof(ICxAuditTrailRecordNameMap))]
	public class CxAuditTrailRecordNameMap : CxAuditTrailOpts<String>, ICxAuditTrailRecordNameMap
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

		public CxAuditTrailRecordNameMap() : base (DefaultRecordName)
		{

		}
	}
}
