using System;
using System.Collections.Generic;
using System.Text;

namespace CxAnalytix.AuditTrails.Crawler.Config
{
	public class CxAuditTrailSupressions : CxAuditTrailOpts<bool>
	{
		public const String SECTION_NAME = "CxAuditTrailSupressions";

		CxAuditTrailSupressions () : base ((x) => false)
		{
		}

	}
}
