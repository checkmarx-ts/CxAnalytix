# Data Record Specification

The record definitions are showing the possible fields provided in a data record.  Some fields may be omitted if they are not provided
by the source data.  (e.g. OSA records or OSA related fields would not be included in a record output if a project was not being scanned
using OSA.)



## SAST Scan Summary Fields

* &#9745; Project Id
* &#9745; Project Name
* &#9745; Team Name
* &#9745; Scan Id
* &#9745; Scan Type
* &#9745; Scan Product
* &#9745; LoC
* &#9745; Failed LoC
* &#9745; File Count
* &#9745; Scan Start
* &#9745; Scan Finish
* &#9745; Scan Risk
* &#9745; Scan Risk Severity
* &#9745; Cx Version
* &#9745; Languages
* &#9745; Preset
* &#9745; High
* &#9745; Medium
* &#9745; Low
* &#9745; Information
* &#9745; Number of policies violated *(Requires M&O to be running and policies assigned to have any meansing)*
* &#9745; Number of rules violated *(Requires M&O to be running and policies assigned to have any meansing)*


## SAST Vulnerability Details

* &#9745; Project Name
* &#9745; Project Id
* &#9745; Team Name
* &#9745; Scan ID
* &#9745; Scan Product
* &#9745; Scan Type
* &#9745; Unique Vulnerability ID
* &#9745; Status
* &#9745; State
* &#9745; Query Name
* &#9745; Severity
* &#9745; DeepLink
* &#9745; FileName
* &#9745; Line
* &#9745; Column
* &#9745; Node ID
* &#9745; Name
* &#9745; Type
* &#9745; Length
* &#9745; Snippet

## OSA Scan Summary

* &#9745; Project Id
* &#9745; Project Name
* &#9745; Team Name
* &#9745; Scan ID 
* &#9745; Scan Start
* &#9745; Scan End
* &#9745; Number of policies violated 
* &#9745; Number of rules violated 
* &#9745; Total policy violations 
* &#9745; HighVulnerabilityLibraries
* &#9745; MediumVulnerabilityLibraries
* &#9745; LowVulnerabilityLibraries
* &#9745; NonVulnerableLibraries
* &#9745; TotalLibraries
* &#9745; TotalHighVulnerabilities
* &#9745; TotalMediumVulnerabilities
* &#9745; TotalLowVulnerabilities
* &#9745; VulnerabilityScore
* &#9745; VulnerableAndOutdated
* &#9745; VulnerableAndUpdated
* &#9745; Legal High
* &#9745; Legal Medium
* &#9745; Legal Low
* &#9745; Legal Unknown
* ~~&#9744; Number of Open Source Detected~~ (removed, all libraries are open source, proprietary/commercial libraries are not recognized)
* ~~&#9744; Number of Unrecognized~~ (removed, this may be "Legal Unknown")


## OSA Vulnerability Details

* &#9745; Project Id
* &#9745; Project Name
* &#9745; Team Name
* &#9745; Scan ID 
* &#9745; Vulnerability ID 
* &#9745; Similarity ID 
* &#9745; CVE Name 
* &#9745; CVE Description 
* &#9745; CVE URL 
* &#9745; CVE Publish Date 
* &#9745; CVE Score 
* &#9745; Recommendation 
* &#9745; Severity 
* &#9745; State 
* &#9745; Library ID
* &#9745; Library Name 
* &#9745; Library Current Version 
* &#9745; Library Current Version Release Date 
* &#9745; Library Newest Version 
* &#9745; Library Newest Version Release Date 
* &#9745; Legal License 
* &#9745; LegalRisk (repeated for each license)

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




