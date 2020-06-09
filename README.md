# CxAnalytix

## What is it?

CxAnalytix, at the current state, is a background process that crawls Checkmarx SAST, OSA, and Management & Orchestration APIs to 
obtain data about vulnerabilities.  The data is then flattened into a JSON format with the intent to be forwarded to a data analytics 
platform for analysis.  Analysis can be performed on the data alone or in aggregate with other sources of data.

Please see the [CxAnalytix Wiki](https://github.com/checkmarx-ts/CxAnalytix/wiki) for information related to obtaining, installing, and configuring CxAnalytix.


## Currently Supported Outputs

### Log File
The log file format writes one JSON document per line, allowing files to be tailed and forwarded to log aggregators such as Splunk.  

### MongoDB
The current MongoDB implementation has been tested to verify functionality against a MongoDB instance.  Testing is planned to verify functionality with other MongoDB API databases (CosmoDB, DocumentDB, etc).  The "partition key" generation feature is in place as a potential method of providing the ability to infinitely scale cloud storage of documents.


## Future Supported Outputs

Documents delivered as messages to AMQP endpoints planned 3Q2020



