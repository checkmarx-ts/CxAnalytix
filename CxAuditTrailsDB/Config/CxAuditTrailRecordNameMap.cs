using System;
using System.Collections.Generic;
using System.Configuration;
using System.Text;

namespace CxAnalytix.CxAuditTrails.DB.Config
{
	public class CxAuditTrailRecordNameMap : CxAuditTrailOpts<String>
	{
		public const String SECTION_NAME = "CxAuditTrailRecords";

		private static String StripNonAlphanumeric (String name)
		{
			StringBuilder sb = new StringBuilder();

			foreach (Char c in name)
			{
				if (Char.IsLetterOrDigit(c))
					sb.Append(c);
			}

			return sb.ToString ();
		}

		public CxAuditTrailRecordNameMap() : base (StripNonAlphanumeric)
		{

		}
	}
}
