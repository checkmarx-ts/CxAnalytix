using CxAnalytix.AuditTrails.Crawler.Contracts;
using CxAnalytix.Configuration.Contracts;
using System;
using System.Composition;
using System.Text;

namespace CxAnalytix.AuditTrails.Crawler.Config
{
	[Export(typeof(ICxAuditTrailRecordNameMap))]
	public class CxAuditTrailRecordNameMap : CxAuditTrailOpts<String>, ICxAuditTrailRecordNameMap
	{

		public CxAuditTrailRecordNameMap() { }

        [ImportingConstructor]
		public CxAuditTrailRecordNameMap(IConfigSectionResolver resolver) : base(DefaultRecordName, resolver) { }

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
