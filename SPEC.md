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
* InstanceId *(Only included if an instance id is configured)*
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
* InstanceId *(Only included if an instance id is configured)*
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


## SCA/OSA Scan Summary

* HighVulnerabilityLibraries
* InstanceId *(Only included if an instance id is configured)*
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


## SCA/OSA Vulnerability Details

* CVEDescription
* CVEName
* CVEPubDate
* CVEScore
* CVEUrl
* InstanceId *(Only included if an instance id is configured)*
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

* CustomFields *(Only included if custom fields are defined for the project.  Structure is dynamically generated using custom field name/value elements.)*
* InstanceId *(Only included if an instance id is configured)*
* Policies *(M&O is required to support policy assignment)*
* Preset
* ProjectId
* ProjectName
* SAST_LastScanDate
* SAST_Scans
* SCA_LastScanDate *(omitted if OSA is not used)*
* SCA_Scans *(omitted if OSA is not used)*
* TeamName

## Policy Violations Details

*This record requires M&O to be installed and policies assigned to projects prior to scans.  Scans performed without a policy assigned will not have a policy
violation record.*

* FirstViolationDetectionDate
* InstanceId *(Only included if an instance id is configured)*
* PolicyId
* PolicyName
* ProjectId
* ProjectName
* RuleCreateDate
* RuleDescription
* RuleId
* RuleName
* RuleType
* ScanId
* ScanProduct
* ScanType
* TeamName
* ViolationName
* ViolationOccurredDate
* ViolationRiskScore
* ViolationSeverity
* ViolationState
* ViolationStatus





