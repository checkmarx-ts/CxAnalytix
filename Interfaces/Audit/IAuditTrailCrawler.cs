using System;
using System.Collections.Generic;
using System.Text;

namespace CxAnalytix.Interfaces.Audit
{
	public interface IAuditTrailCrawler
	{

		void crawl(DateTime since);
	}
}
