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
		void CxActivity_dbo_Audit_DataRetention(DateTime sinceDate, IOutput output);
		void CxActivity_dbo_Audit_Logins(DateTime sinceDate, IOutput output);
		void CxActivity_dbo_Audit_Presets(DateTime sinceDate, IOutput output);
		void CxActivity_dbo_Audit_Projects(DateTime sinceDate, IOutput output);
		void CxActivity_dbo_Audit_Queries(DateTime sinceDate, IOutput output);
		void CxActivity_dbo_Audit_QueriesActions(DateTime sinceDate, IOutput output);
		void CxActivity_dbo_Audit_Reports(DateTime sinceDate, IOutput output);
		void CxActivity_dbo_Audit_ScanRequests(DateTime sinceDate, IOutput output);
		void CxActivity_dbo_Audit_Scans(DateTime sinceDate, IOutput output);
		void CxActivity_dbo_Audit_Users(DateTime sinceDate, IOutput output);

		public bool IsDisabled { get; }

	}
}
