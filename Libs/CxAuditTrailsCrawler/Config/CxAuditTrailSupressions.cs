using CxAnalytix.AuditTrails.Crawler.Contracts;
using CxAnalytix.Configuration.Contracts;
using System;
using System.Collections.Generic;
using System.Composition;
using System.Text;

namespace CxAnalytix.AuditTrails.Crawler.Config
{
	[Export(typeof(ICxAuditTrailSuppressions))]
	public class CxAuditTrailSupressions : CxAuditTrailOpts<bool>, ICxAuditTrailSuppressions
	{

		public CxAuditTrailSupressions() { }

		[ImportingConstructor]
		public CxAuditTrailSupressions(IConfigSectionResolver resolver) : base((x) => false, resolver) { }



	}
}
