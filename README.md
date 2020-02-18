# CxAnalytix

CxAnalytix can be invoked manually or run in the background to crawl the Checkmarx server for scans.  The scan report data is then flattened into key-value pairs using a JSON format.  The JSON format is designed to be imported into analytics platforms.


# Installation

The program can be built from source or downloaded as a zip archive. The resulting binaries can be executed to perform the export of analytics data.

.Net Core Runtime 2.1 is required to execute CxAnalytix.  Install using Chocolately:

`choco install dotnetcore-runtime.install --version=2.1.15`

*It is suggested that the configuration options be configured before the first execution. The configuration is discussed below.*

## Command Line Installation

There is no installation currently required for the command line.  It can be executed on-demand.  It executes only one scan and then exits.

## Windows Service Installation

1. Open Powershell or Windows command prompt as administrator.
2. Execute `sc.exe create CxAnalytix binpath="{folder path}\CxAnalytixService.exe"`

The service can be started or stopped from the service control manager.  The service will execute scans periodically while it is running.


# Configuration

Configuration files for each executable should be located in the same directory as the executable.

## Command Line Configuration

The command line uses two configuration files:

`CxAnalytixCLI.log4net` - The log4net configuration.

`dotnet.exe.config` - The operation configuration file.

## Windows Service Configuration

*This is not yet supported*


## Log4Net Configuration

The Log4Net configuration file should be modified only to change the output path of the generated log files.  It is an XML configuration where elements with the name "appender" with a "type" attribute set to the string "log4net.Appender.RollingFileAppender" contain the file path that determines where the logs are written.

The "file" element in each of the "appender" elements described above can be configured to place the logs in an ansolute or a relative path.  Modify the path portion of the output name to place the log in the desired location.  Ensure all directories in the specified path are created.

## Operation Configuration

The operation configuration file is an XML file with several elements in the "configuration" element that control the operation of the running program.

### Connection Configuration

An example connection configuration element is shown below:

```
<CxConnection URL="http://www.foo.com" TimeoutSeconds="600" ValidateCertificates="true" />
```

Each attribute can be configured as follows:

| Attribute | Description |
| --- | --- |
|URL| The URL to the Checkmarx SAST server. |
|mnoURL| The URL for M&O if different from SAST.  If empty, the SAST URL will be used.
|TimeoutSeconds|The number of seconds to wait before assuming a call to the REST API times out.|
|ValidateCertificates| Set to true to perform SSL certificate validation.  Set to false to disable certificate validation.


### Credentials Configuration

An example of the unencryped credentials configuration element is shown below:

```
<CxCredentials Username="foo" Password ="bar" Token="" />
```
Note that at the first run of the program, this element is encrypted.  To change the configuration parameters, replace the encrypted element with the unencrypted element as shown above.  Each attribute can be configured as follows:

| Attribute | Description |
| --- | --- |
| Username | The Checkmarx SAST username to use when authenticating with the Checkmarx API. |
|Password| The password for the user account to use when authenticating with the Checkmarx API. |
|Token| Not currently supported.|


### Execution Configuration

An example of the execution configuration element is shown below:


```
  <CxAnalyticsService ConcurrentThreads="2" StateDataStoragePath=".\"
                      ProcessPeriodMinutes="120"
                      OutputFactoryClassPath="CxAnalytics.Out.Log4NetOutput.LoggerOutFactory, Log4NetOutput"
                      SASTScanSummaryRecordName="RECORD_SAST_Scan_Summary"
                      SASTScanDetailRecordName="RECORD_SAST_Scan_Detail"
                      SCAScanSummaryRecordName="RECORD_SCA_Scan_Summary"
                      SCAScanDetailRecordName="RECORD_SCA_Scan_Detail"
                      ProjectInfoRecordName="RECORD_Project_Info"
                      PolicyViolationsRecordName="RECORD_Policy_Violations"
                      />
```

Each attribute can be configured as follows:

| Attribute | Description |
| --- | --- |
|ConcurrentThreads|The number of reports that are processed concurrently.|
|StateDataStoragePath| A path to a folder where the state data that is persisted between each scan is stored.|
|ProcessPeriodMinutes| *Ignored by the CxAnalytixCLI.* The number of minutes the service waits between performing scans for new scan results.|
|OutputFactoryClassPath| Currently only supports the value "CxAnalytics.Out.Log4NetOutput.LoggerOutFactory, Log4NetOutput".  Other values will prevent the program from operating.|
|SASTScanSummaryRecordName|Set to the record name for the SAST Scan Summary record as understood by the Output Factory implementation.  This currently matches the name of a Log4Net logger.|
|SASTScanDetailRecordName|Set to the record name for the SAST Scan Detail record as understood by the Output Factory implementation.  This currently matches the name of a Log4Net logger.|
|SCAScanSummaryRecordName|Set to the record name for the  SCA Scan Summary record as understood by the Output Factory implementation.  This currently matches the name of a Log4Net logger.
|SCAScanDetailRecordName|Set to the record name for the SCA Scan Detail record as understood by the Output Factory implementation.  This currently matches the name of a Log4Net logger.
|ProjectInfoRecordName|Set to the record name for the Project Info record as understood by the Output Factory implementation.  This currently matches the name of a Log4Net logger.
|PolicyViolationsRecordName|Set to the record name for the Policy Violations record as understood by the Output Factory implementation.  This currently matches the name of a Log4Net logger.


# Command Line Execution

To execute CxAnalytix from the command line, open a command prompt or Powershell in the program folder and execute the following:

`dotnet CxAnalytixCLI.dll`

On the initial execution, the state file is written into the configured state storage directory with the filename `CxAnalytixExportState.json`.  Subsequent executions will only pick up scans that are new since the last execution of the program.  To reset the program and re-process all scans, delete `CxAnalytixExportState.json` and execute the program again.


# Forwarding to Splunk

## Configuring the Universal Forwarder

Follow the instructions supplied by Splunk for setting up the Universal Forwarder appropriate for your environment.  Once the forwarder is able to connect to your Splunk instance, create the `inputs.conf` file at `\etc\apps\splunkclouduf\default\inputs.conf`.  In the `inputs.conf` file, create monitoring stanzas appropriate for each type of record. An example of `inputs.conf`:


```
[monitor://{path to logs}/CxAnalytixService*log*]
sourcetype=CxAnalytix_app

[monitor://{path to logs}/sast_scan_summary*log*]
sourcetype=CxAnalytix_sast_summary

[monitor://{path to logs}/sast_scan_detail*log*]
sourcetype=CxAnalytix_sast_detail

[monitor://{path to logs}/sast_project_info*log*]
sourcetype=CxAnalytix_sast_project_info

```


## Configuring the Source Types on the Server

The source types on the Splunk server need to be configured to appropriately parse JSON.  This can be done using `props.conf` or through the Splunk UI.  (The use of `props.conf` on the server side is only supported in Splunk Enterprise.)  A source type should be created the matches each record output source types as defined in `inputs.conf`.  The following 2 configuration options need to be added to the sourcetype:

```
LINE_BREAKER=([\r\n]+)\
KV_MODE=json
```


