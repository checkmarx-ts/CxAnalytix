![Build Badge](https://circleci.com/gh/checkmarx-ts/CxAnalytix.svg?style=shield)

# CxAnalytix

CxAnalytix is a background process that crawls Checkmarx SAST, OSA, and Management & Orchestration APIs to 
obtain data about vulnerabilities.  The data is then flattened into a JSON format with the intent to be forwarded to a data analytics 
platform for analysis.  Analysis can be performed on the data alone or in aggregate with other sources of data.

The fields available in generated documents can be found in the [data field specification](https://github.com/checkmarx-ts/CxAnalytix/wiki/SPEC.md).

## Getting Started

### Dependencies

CxAnalytix is built on .Net Core and is therefore capable of running on Windows or Linux.  

There are several installation variations:

* A Windows service
* A Linux daemon
* A command line executable

### Installation

Please refer to the [Installation](https://github.com/checkmarx-ts/CxAnalytix/wiki/Installation-Home) wiki page


## Additional Documentation

Please see the [CxAnalytix Wiki](https://github.com/checkmarx-ts/CxAnalytix/wiki) for information related to obtaining, installing, and configuring CxAnalytix.


## Version History
* 1.3.1
	* FEATURES
		* Platform-specific tarballs are now created.  This is to address the dynamic loading of DPAPI that .Net Core apparently doesn't handle well in cross-platform builds.
		* Pseudo-transactions are now off by default.
		* New data fields added to scan summary and scan detail records.
	* BUG FIXES
		* Issue 85 - Malformed AMQP config written on first run, preventing subsequent runs without removing the malformed config and commenting out the AMQP config class references.
* 1.3.0
	* FEATURES
		* Issue 10 - Output can now be routed to AMQP endpoints
* 1.2.5
	* FEATURES
		* Issue 52 - Transactional writes have been implemented as [Pseudo Transactions](https://github.com/checkmarx-ts/CxAnalytix/wiki/Pseudo-Transactions)
	* BUG FIXES
		* An issue with crawls aborting on SAST systems not licensed for OSA was re-introduced in 1.2.2 and has been fixed.
* 1.2.4
	* BUG FIXES
		* Stability fix for cases where M&O did not return policy violations as expected
		* Build change to not build self-contained; this was causing issues on some Linux distros
* 1.2.3
	* FEATURES
		* Issue 57 - Filtering scans crawled via Team and Project regex matching
	* BUG FIXES
		* Issue 17 - Updated the docker image to better support persisting the state files
* 1.2.2
	* FEATURES
		* Fields added to the output records
			* Project Information
				* LastCrawlDate
			* Policy Violation Details
				* ViolationId
		* A basic [regression testing utility](https://github.com/checkmarx-ts/CxAnalytix/wiki/Development-Home) was added to test that data extraction is consistent between versions.  This is primarily targeted for developer use.
	* BUG FIXES
		* Issue 51 - Timestamp of date to check for last scan is recorded as the finish date of the last scan found during the current crawl rather than the date of the current crawl.
		* Issue 53 - Authorization token refresh improvements
		* Stealth fix during development - NodeLine would be excluded from the SAST Vulnerability Details record under certain conditions 
* 1.2.1
	* BUG FIXES
		* Issue 60 - A DB table row with a column containing a NULL value threw an exception and caused the DB crawl to end prematurely.
* 1.2.0
	* FEATURES
		* New feature to extract audit events by crawling audit log tables in CxActivity and CxDB.  This feature is limited to use in systems that can make a connection directly to the CxSAST DB.
* 1.1.7
	* BUG FIXES
		* Issue 31 - No time delay between queries for report generation status.
* 1.1.6
	* BUG FIXES
		* Issue 26 - OSA scan details incomplete or missing
	* FEATURES
		* The rolling file log naming convention should cause daily log rotation as well as 100MB max log file sizes by default.
* 1.1.5
  * FEATURES
    * Added the ability to dump all network I/O to the application log.
    * Improved error handling and exception logging for troubleshooting purposes.
  * BUG FIXES
    * Issues #21, 22 - Networking implementation caused issues on some versions of Windows server.
* 1.1.4
  * FEATURES
    * Added EngineStart/EngineFinished fields to the scan summary; no-change scans will be indicated with DateTime.MinValue
  * BUG FIXES
    * Issue #20: Date parsing error in non-US locale
* 1.1.3
  * BUG FIXES
    * Issue #18: Error when attempting to retrieve policy violation data from SAST 9.0
* 1.1.2
  * FEATURES
    * Dockerfile now available as a release artifact
    * [Docker base image](https://hub.docker.com/r/checkmarxts/cxanalytix) pushed to Docker Hub as part of the build 
* 1.1.1
   * FEATURES
      * Issue #9: Resolve config values from environment variables (see the Wiki for [CxConnection](https://github.com/checkmarx-ts/CxAnalytix/wiki/CxConnection), [CxCredentials](https://github.com/checkmarx-ts/CxAnalytix/wiki/CxCredentials), and [CxAnalyticsService](https://github.com/checkmarx-ts/CxAnalytix/wiki/CxAnalyticsService))
    * BUG FIXES
      * Issue #6: Now compatible with SAST 9.0
* 1.1.0
    * FEATURES
      * Issue #4: MongoDB is now available as an output destination.
      * Issue #5: Add instance identifier to each record.
      * Issue #7: Add project custom fields to the output.
* 1.0.0
    * Initial Release
    * FEATURES
        * Output to flat log files
        * Support for CxSAST 8.9 APIs

## Contributing

We appreciate feedback and contribution to this repo! Before you get started, please see the following:

- [Checkmarx general contribution guidelines](CONTRIBUTING.md)
- [Checkmarx code of conduct guidelines](CODE-OF-CONDUCT.md)

## Support + Feedback

Include information on how to get support. Consider adding:

- Use [Issues](https://github.com/checkmarx-ts/CxAnalytix/issues) for code-level support
- For installation assistance, schedule time with the Checkmarx Professional Services team


## License

Project License can be found [here](LICENSE)


