using System;
using CxAnalytix.Interfaces.Outputs;
using CxAnalytix.Interfaces.Audit;

namespace CxAnalytix.CxAuditTrails
{
	public class DBCrawler : IAuditTrailCrawler
	{

		public DBCrawler (IOutputFactory outFactory)
		{

		}


		public void crawl(DateTime since)
		{
			throw new NotImplementedException();
		}
	}
}
