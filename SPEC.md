# Data Record Specification

The record definitions are showing the possible fields provided in a data record.  Some fields may be omitted if they are not provided
by the source data.  (e.g. OSA records or OSA related fields would not be included in a record output if a project was not being scanned
using OSA.)



## SAST Scan Summary Fields


* CxVersion
* DeepLink
* FailedLinesOfCode
* FileCount
* Information
* Initiator
* Languages
* LinesOfCode
* Low
* PoliciesViolated *(Requires M&O to be running and policies assigned to have any meansing)*
* PolicyViolations *(Requires M&O to be running and policies assigned to have any meansing)*
* Preset
* ProjectId
* ProjectName
* ReportCreationTime
* RulesViolated
* ScanComments
* ScanFinished
* ScanId
* ScanProduct
* ScanRisk
* ScanRiskSeverity
* ScanStart
* ScanTime
* ScanType
* SourceOrigin
* TeamName


## SAST Vulnerability Details

* FalsePositive
* NodeCodeSnippet
* NodeColumn
* NodeFileName
* NodeId
* NodeLength
* NodeLine
* NodeName
* NodeType
* PathId
* ProjectId
* ProjectName
* QueryCategories
* QueryCweId
* QueryGroup
* QueryId
* QueryLanguage
* QueryName
* QuerySeverity
* QueryVersionCode
* Remark
* ResultDeepLink
* ResultId
* ResultSeverity
* ScanFinished
* ScanId
* ScanProduct
* ScanType
* SimilarityId
* SinkColumn
* SinkFileName
* SinkLine
* State
* Status
* TeamName
* VulnerabilityId


## SCA Scan Summary

* HighVulnerabilityLibraries
* LegalHigh
* LegalLow
* LegalMedium
* LowVulnerabilityLibraries
* MediumVulnerabilityLibraries
* NonVulnerableLibraries
* PoliciesViolated
* PolicyViolations
* ProjectId
* ProjectName
* RulesViolated
* ScanFinished
* ScanId
* ScanStart
* TeamName
* TotalHighVulnerabilities
* TotalLibraries
* TotalLowVulnerabilities
* TotalMediumVulnerabilities
* VulnerabilityScore
* VulnerableAndOutdated
* VulnerableAndUpdated


## SCA Vulnerability Details

* CVEDescription
* CVEName
* CVEPubDate
* CVEScore
* CVEUrl
* LibraryId
* LibraryLatestReleaseDate
* LibraryLatestVersion
* LibraryLegalRisk_{License & Version} *(Field name is dynamically generated)*
* LibraryLicenses
* LibraryName
* LibraryReleaseDate
* LibraryVersion
* ProjectId
* ProjectName
* Recommendation
* ScanFinished
* ScanId
* ScanRiskSeverity
* SimilarityId
* State
* TeamName
* VulnerabilityId



## Project Information

* &#9745; Project Id
* &#9745; Project Name 
* &#9745; Team Name 
* &#9745; Preset Name 
* &#9745; SAST Last Scan Date
* &#9745; SAST Total Scans
* &#9745; SCA Last Scan Date *(omitted if OSA is not used)*
* &#9745; SCA Total Scans *(omitted if OSA is not used)*
* &#9745; Policies *(M&O is required to support policy assignment)*

## Policy Violations Details

*This record requires M&O to be installed and policies assigned to projects prior to scans.  Scans performed without a policy assigned will not have a policy
violation record.*

* &#9745; Project Name
* &#9745; Team Name (added)
* &#9745; Scan ID
* &#9745; Scan Product 
* &#9745; Scan Type
* &#9745; Policy Id
* &#9745; Policy Name 
* &#9745; Rule ID
* &#9745; Rule Name 
* &#9745; Rule Description
* &#9745; Rule Type
* &#9745; Rule Create Date
* &#9745; First Violation Detection Date
* &#9745; Violation Name
* &#9745; Violation Occurrence Date
* &#9745; Violation Risk Score
* &#9745; Violation Severity
* &#9745; Violation Source
* &#9745; Violation State
* &#9745; Violation Status
* &#9745; Violation Type
* ~~&#9744; Number of occurrences~~ (removed: the number of occurrences does not make sense because each occurrence can have a different source)




