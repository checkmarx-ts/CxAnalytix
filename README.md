# CxAnalytix

CxAnalytix is a background process that crawls Checkmarx SAST, OSA, and Management & Orchestration APIs to 
obtain data about vulnerabilities.  The data is then flattened into a JSON format with the intent to be forwarded to a data analytics 
platform for analysis.  Analysis can be performed on the data alone or in aggregate with other sources of data.

The fields available in generated documents can be found in the [specification](SPEC.md).

## Description

Details description of the solution and usage overview.

## Getting Started

### Dependencies

CxAnalytix is built on .Net Core and is therefore capable of running on Windows or Linux.  

There are several installation variations:

* A Windows service
* A Linux daemin
* A command line executable

### Installation

Please refere to the [Installation](https://github.com/checkmarx-ts/CxAnalytix/wiki/Installation) wiki page


## Additional Documentation

Please see the [CxAnalytix Wiki](https://github.com/checkmarx-ts/CxAnalytix/wiki) for information related to obtaining, installing, and configuring CxAnalytix.


## Version History

* [1.1](https://github.com/checkmarx-ts/CxAnalytix/releases/tag/v1.1.0)
    * FEATURES
      * Issue #4: MongoDB is now available as an output destination.
      * Issue #5: Add instance identifier to each record.
      * Issue #7: Add project custom fields to the output.
* 1.0
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


## License

Project Lisense can be found [here](LICENSE)


