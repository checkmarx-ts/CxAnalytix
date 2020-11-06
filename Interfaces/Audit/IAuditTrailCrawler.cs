using CxAnalytix.Interfaces.Outputs;
using System;
using System.Collections.Generic;
using System.Text;

namespace CxAnalytix.Interfaces.Audit
{
	public interface IAuditTrailCrawler
	{
		void Crawl_CxDB_accesscontrol_AuditTrail(DateTime sinceDate, IOutput output);
	}
}
