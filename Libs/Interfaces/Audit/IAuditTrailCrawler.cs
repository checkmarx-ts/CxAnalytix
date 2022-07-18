using CxAnalytix.Interfaces.Outputs;
using System;
using System.Collections.Generic;
using System.Text;

namespace CxAnalytix.Interfaces.Audit
{
	public interface IAuditTrailCrawler
	{
		void CxDB_accesscontrol_AuditTrail(DateTime sinceDate, IOutputTransaction trx, IRecordRef record);
		void CxActivity_dbo_AuditTrail(DateTime sinceDate, IOutputTransaction trx, IRecordRef record);
		void CxActivity_dbo_Audit_DataRetention(DateTime sinceDate, IOutputTransaction trx, IRecordRef record);
		void CxActivity_dbo_Audit_Logins(DateTime sinceDate, IOutputTransaction trx, IRecordRef record);
		void CxActivity_dbo_Audit_Presets(DateTime sinceDate, IOutputTransaction trx, IRecordRef record);
		void CxActivity_dbo_Audit_Projects(DateTime sinceDate, IOutputTransaction trx, IRecordRef record);
		void CxActivity_dbo_Audit_Queries(DateTime sinceDate, IOutputTransaction trx, IRecordRef record);
		void CxActivity_dbo_Audit_QueriesActions(DateTime sinceDate, IOutputTransaction trx, IRecordRef record);
		void CxActivity_dbo_Audit_Reports(DateTime sinceDate, IOutputTransaction trx, IRecordRef record);
		void CxActivity_dbo_Audit_ScanRequests(DateTime sinceDate, IOutputTransaction trx, IRecordRef record);
		void CxActivity_dbo_Audit_Scans(DateTime sinceDate, IOutputTransaction trx, IRecordRef record);
		void CxActivity_dbo_Audit_Users(DateTime sinceDate, IOutputTransaction trx, IRecordRef record);

		public bool IsDisabled { get; }

	}
}
