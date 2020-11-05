using System;
using System.Collections.Generic;
using System.Text;

namespace CxAnalytix.AuditTrails.Crawler.Interfaces
{
	public interface IAuditTrailCrawler
	{

		void crawl(DateTime since);
	}
}
