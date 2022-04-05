[![build-ci](https://github.com/checkmarx-ts/CxAnalytix/actions/workflows/build-ci.yml/badge.svg)](https://github.com/checkmarx-ts/CxAnalytix/actions/workflows/build-ci.yml)

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

Version history has been moved to [CHANGELOG.md](CHANGELOG.md)

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


