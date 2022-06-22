using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CxAnalytix.AuditTrails.Crawler.Contracts
{
    public interface ICxAuditTrailOpts<T>
    {
        T CxDB_accesscontrol_AuditTrail { get; set; }
        T CxActivity_dbo_AuditTrail { get; set; }
        T CxActivity_dbo_Audit_DataRetention { get; set; }
        T CxActivity_dbo_Audit_Logins { get; set; }
        T CxActivity_dbo_Audit_Presets { get; set; }
        T CxActivity_dbo_Audit_Projects { get; set; }
        T CxActivity_dbo_Audit_Queries { get; set; }
        T CxActivity_dbo_Audit_QueriesActions { get; set; }
        T CxActivity_dbo_Audit_Reports { get; set; }
        T CxActivity_dbo_Audit_ScanRequests { get; set; }
        T CxActivity_dbo_Audit_Scans { get; set; }
        T CxActivity_dbo_Audit_Users { get; set; }
    }
}
