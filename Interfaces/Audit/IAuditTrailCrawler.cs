using CxAnalytix.Interfaces.Outputs;
using System;
using System.Collections.Generic;
using System.Text;

namespace CxAnalytix.Interfaces.Audit
{
	public interface IAuditTrailCrawler
	{
		void CxDB_accesscontrol_AuditTrail(DateTime sinceDate, IOutput output);
		void CxActivity_dbo_AuditTrail(DateTime sinceDate, IOutput output);
	}
}
