using System;
using CxAnalytix.Interfaces.Outputs;
using CxAnalytix.Interfaces.Audit;

namespace CxAnalytix.CxAuditTrails
{
	public class DBCrawler : IAuditTrailCrawler
	{

		public DBCrawler ()
		{

		}

		public void Crawl_CxDB_accesscontrol_AuditTrail(DateTime sinceDate, IOutput output)
		{
			throw new NotImplementedException();
		}
	}
}
