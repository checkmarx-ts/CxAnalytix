# CxAnalytix

## What is it?

CxAnalytix, at the current state, is a background process that crawls Checkmarx SAST, SCA (OSA), and Management & Orchestration APIs to 
obtain data about vulnerabilities.  The data is then flattened into a JSON format with the intent to be forwarded to a data analytics 
platform for analysis.  Analysis can be performed on the data alone or in aggregate with other sources of data.

## Plan for Future
The current implementation writes the flattened data as JSON to a file.  The files can be tailed and data forwarded to syslog 
aggregators such as Splunk or any other platform that can use JSON collections of key/value pairs.  The architecture is such that 
the flattened data is provided as messages to an output implementation, allowing for future implementations for use-cases such as writing 
to document databases, message queuing, writing in columnar file formats (Parquet, ORC, etc) for Hadoop, etc.

Please see the [CxAnalytix Wiki](wiki) for information related to obtaining, installing, and configuring CxAnalytix.





