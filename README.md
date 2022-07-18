[![build-ci](https://github.com/checkmarx-ts/CxAnalytix/actions/workflows/build-ci.yml/badge.svg)](https://github.com/checkmarx-ts/CxAnalytix/actions/workflows/build-ci.yml)

# CxAnalytix

CxAnalytix is a background process that crawls and extracts vulnerability data from Checkmarx products.  Currently it supports:

* Checkmarx SAST, including:
    * OSA (optional)
    * Management & Orchestration (optional)
* Checkmarx SCA


The vulnerability data is then transformed into a flattened JSON document and stored for at-scale data analysis.  Pluggable data transports are implemented for storage flexibility; data can be easily stored and accessed locally or transported to data lakes for more advanced analysis.

The fields available JSON documents can be found in the [data field specification](https://github.com/checkmarx-ts/CxAnalytix/wiki/SPEC.md).


## NOTE: 2.0.0 PRE-RELEASE

**CxAnalytix v1.x configurations are not backwards compatible with v2.0.0 configurations.**

*THE WIKI DOES NOT CURRENTLY REFLECT THE DOCUMENTATION FOR THE v2.0.0 PRE-RELEASE.  THE WIKI UPDATE WILL COINCIDE WITH THE RELEASE OF v2.0.0.  IF THERE IS A NEED TO IMMEDIATELY EVALUATE v2.0.0, PLEASE SCHEDULE TIME WITH CHECKMARX PROFESSIONAL SERVICES.*

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


