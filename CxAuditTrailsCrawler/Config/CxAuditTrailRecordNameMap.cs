using System;
using System.Collections.Generic;
using System.Configuration;
using System.Text;

namespace CxAnalytix.AuditTrails.Crawler.Config
{
	public class CxAuditTrailRecordNameMap : CxAuditTrailOpts<String>
	{
		public const String SECTION_NAME = "CxAuditTrailRecords";

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
