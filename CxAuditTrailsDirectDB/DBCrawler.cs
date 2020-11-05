using System;
using CxAnalytix.TransformLogic.Interfaces;

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
