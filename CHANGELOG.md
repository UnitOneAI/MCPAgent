# CHANGELOG 📝

All notable changes to the Windows 365 MCP Server will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [1.0.0] - 2025-01-18

### Added
- **🎉 First Public Release**
- Complete Model Context Protocol (MCP) server for Windows 365 Cloud PC management
- Azure Identity authentication for secure Microsoft Graph API access via STDIO transport
- **Tools** (13 total):
  - Cloud PC discovery and management (`DiscoverCloudPCs`, `GetCloudPCDetails`, `RebootCloudPC`, `EndGracePeriod`)
  - License management (`GetWindows365Licenses`, `CheckUserLicenses`, `AssignLicense`, `UnassignLicense`)
  - Provisioning policy management (`GetProvisioningPolicies`, `GetProvisioningPolicyDetails`)
  - User and group management (`SearchUsers`, `GetUserDetails`, `SearchGroups`)
- **Resources** (7 total):
  - `windows365://cloudpcs` - Complete Cloud PC inventory
  - `windows365://cloudpc/{id}` - Individual Cloud PC details
  - `windows365://provisioning-policies` - Provisioning policy configurations
  - `windows365://licenses` - License availability and usage
  - `windows365://user-licenses/{userId}` - User-specific license details
  - `windows365://groups` - Entra ID groups listing
  - `windows365://tenant-summary` - Tenant overview and capabilities
- **Prompts** (6 total):
  - `cloud_pc_troubleshoot` - Diagnostic workflow for Cloud PC issues
  - `provisioning_policy_analysis` - Policy configuration analysis
  - `license_optimization` - License usage optimization recommendations
  - `deployment_planning` - Windows 365 deployment planning guidance
  - `user_provisioning` - Cloud PC provisioning workflow for new users
  - `security_assessment` - Configuration security review
- Complete provisioning workflow automation following Microsoft's License → Group → Policy pattern
- Support for all Windows 365 Enterprise and Business SKUs
- Comprehensive error handling with detailed logging and structured JSON responses
- .NET 9.0 support with modern C# features including collection expressions
- MIT License for open-source distribution
- Complete project metadata for public distribution

### Technical Features
- **Authentication**: Azure Identity DefaultAzureCredential with Microsoft Graph API integration
- **Error Handling**: Comprehensive exception handling with user-friendly error messages
- **Documentation**: XML documentation generation enabled for complete API reference
- **Code Quality**: Optimized using statements with global using directives
- **Dependency Management**: Latest stable versions of Microsoft Graph and Azure Identity libraries

### Dependencies
- Microsoft Graph 5.56.0
- Azure Identity 1.12.0
- ModelContextProtocol 0.3.0-preview.4
- .NET 9.0 runtime

### Notes
This project demonstrates UNITONE's approach to enterprise AI governance, making AI workloads visible, controllable, and safe at enterprise scale.
