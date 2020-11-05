using System;
using System.Collections.Generic;
using System.Text;

namespace CxAnalytix.CxAuditTrails.Config
{
	public class CxAuditTrailSupressions : CxAuditTrailOpts<bool>
	{
		public const String SECTION_NAME = "CxAuditTrailSupressions";

		CxAuditTrailSupressions () : base ((x) => false)
		{
		}

	}
}
