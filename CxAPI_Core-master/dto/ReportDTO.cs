using System;
using System.Collections.Generic;
using System.Text;

namespace CxAPI_Core.dto
{
    class ReportRequest
    {
        public string reportType { get; set; }
        public long scanId { get; set; }
    }

    public partial class ReportReady
    {
        public Link Link { get; set; }
        public string ContentType { get; set; }
        public ReportStatus Status { get; set; }
    }

    public partial class Link
    {
        public string Rel { get; set; }
        public string Uri { get; set; }
    }

    public partial class ReportState
    {
        public ReportLink Link { get; set; }
        public string ContentType { get; set; }
        public ReportStatus Status { get; set; }

    }
    public partial class ReportTrace
    {
        public long projectId { get; set; }
        public long scanId { get; set; }
        public long reportId { get; set; }
        public string reportType {get; set;}
        public string projectName { get; set; }
        public DateTimeOffset? scanTime { get; set; }
        public bool isRead { get; set; }

        public ReportTrace(long projectId, string projectName, DateTimeOffset? scanTime, long pScanId, long preportId, string reportType)
        {
            this.projectId = projectId;
            this.scanId = pScanId;
            this.reportId = preportId;
            this.projectName = projectName;
            this.scanTime = scanTime;
            this.reportType = reportType;
            this.isRead = false;
        }
    }
    public partial class ReportLink
    {
        public string Rel { get; set; }
        public string Uri { get; set; }
    }
    public partial class ReportStatus
    {
        public long Id { get; set; }
        public string Value { get; set; }
    }

    public partial class ReportResult
    {
        public long ReportId { get; set; }
        public Links Links { get; set; }
    }

    public partial class Links
    {
        public Report Report { get; set; }
        public Report Status { get; set; }
    }

    public partial class Report
    {
        public string Rel { get; set; }
        public string Uri { get; set; }
    }
    public partial class ReportResultNew
    {
        public long projectId { get; set; }
        public long scanId { get; set; }
        public int state { get; set; }
        public string status { get; set; }
        public string Severity { get; set; }
        public string Group { get; set; }
        public string Query { get; set; }
    }
    public partial class ReportResultAll
    {
        public string projectName { get; set; }
        public string teamName { get; set; }
        public string presetName { get; set; }
        public DateTime scanDate { get; set; }
        public long projectId { get; set; }
        public long scanId { get; set; }
        public int state { get; set; }
        public string status { get; set; }
        public string Severity { get; set; }
        public string Group { get; set; }
        public string Query { get; set; }
    }
    public partial class ReportResultExtended
    {
        public string projectName { get; set; }
        public string teamName { get; set; }
        public string presetName { get; set; }
        public long similarityId { get; set; }
        public long resultId { get; set; }
        public long reportId { get; set; }
        public long pathId { get; set; }
        public long nodeId { get; set; }
        public long projectId { get; set; }
        public long queryId { get; set; }
        public long scanId { get; set; }
        public int state { get; set; }
        public string status { get; set; }
        public string Severity { get; set; }
        public string Group { get; set; }
        public string Query { get; set; }
        public int lineNo { get; set; }
        public int column { get; set; }
        public string firstLine { get; set; }
        public string fileName { get; set; }
        public DateTime scanDate { get; set; }
    }
    public partial class ReportOutput
    {
        public string ProjectName { get; set; }
        public int StartHigh { get; set; }
        public int StartMedium { get; set; }
        public int StartLow { get; set; }
        public int LastHigh { get; set; }
        public int LastMedium { get; set; }
        public int LastLow { get; set; }
        public int NewHigh { get; set; }
        public int NewMedium { get; set; }
        public int NewLow { get; set; }
        public int DiffHigh { get; set; }
        public int DiffMedium { get; set; }
        public int DiffLow { get; set; }
        public int NotExploitable { get; set; }
        public int Confirmed { get; set; }
        public int ToVerify { get; set; }
        public DateTimeOffset firstScan { get; set; }
        public DateTimeOffset lastScan { get; set; }
        public int ScanCount { get; set; }

    }
    public partial class ReportOutputExtended
    {
        public string ProjectName { get; set; }
        public string Team { get; set; }
        public int StartHigh { get; set; }
        public int StartMedium { get; set; }
        public int StartLow { get; set; }
        public int StartNotExploitable { get; set; }
        public int StartConfirmed { get; set; }
        public int StartToVerify { get; set; }
        public int StartFixed { get; set; }
        public int StartOthers { get; set; }
        public int LastHigh { get; set; }
        public int LastMedium { get; set; }
        public int LastLow { get; set; }
        public int NewHigh { get; set; }
        public int NewMedium { get; set; }
        public int NewLow { get; set; }
        public int DiffHigh { get; set; }
        public int DiffMedium { get; set; }
        public int DiffLow { get; set; }
        public int LastNotExploitable { get; set; }
        public int LastConfirmed { get; set; }
        public int LastToVerify { get; set; }
        public int LastFixed { get; set; }
        public int LastOthers { get; set; }
        public DateTimeOffset firstScan { get; set; }
        public DateTimeOffset lastScan { get; set; }
        public int ScanCount { get; set; }

    }
    public partial class ReportStaging
    {
        public long ProjectId { get; set; }
        public long ScanId { get; set; }
        public string ProjectName { get; set; }
        public string TeamName { get; set; }
        public DateTimeOffset dateTime { get; set; }
        public int High { get; set; }
        public int Medium { get; set; }
        public int Low { get; set; }
        public int ScanCount { get; set; }
    }

}