[![build-ci](https://github.com/checkmarx-ts/CxAnalytix/actions/workflows/build-ci.yml/badge.svg)](https://github.com/checkmarx-ts/CxAnalytix/actions/workflows/build-ci.yml)

# CxAnalytix

CxAnalytix is a background process that crawls and extracts vulnerability data from Checkmarx products.  Currently it supports:

* Checkmarx SAST, including:
    * OSA (optional)
    * Management & Orchestration (optional)
* Checkmarx SCA
* Checkmarx One
	* SAST Scans
	* SCA Scans


The vulnerability data is then transformed into a flattened JSON document and stored for at-scale data analysis.  Pluggable data transports are implemented for storage flexibility; data can be easily stored and accessed locally or transported to data lakes for more advanced analysis.

The fields available JSON documents can be found in the `Data Field Specification` section of the PDF manual.

## NOTE: 2.0.0 RELEASE

**CxAnalytix v1.x configurations are not backwards compatible with v2.0.0 configurations. Please review the configuration PDF manual for details.**

As of release 2.0.0, a PDF manual is included with each release.  The [CxAnalytix Wiki](https://github.com/checkmarx-ts/CxAnalytix/wiki) is available for reference
for versions prior to 2.0.0.  The CxAnalytix Wiki will no longer be maintained as of release 2.0.0.

## Getting Started


1. Download the [latest release](https://github.com/checkmarx-ts/CxAnalytix/releases) appropriate for your platform.
2. Download the PDF manual and review the `Quickstart` chapter.


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


