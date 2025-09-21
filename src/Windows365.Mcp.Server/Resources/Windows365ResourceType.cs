using ModelContextProtocol.Protocol;
using Windows365.Mcp.Server.Services;

namespace Windows365.Mcp.Server.Resources;

[McpServerResourceType]
public class Windows365ResourceType
{
    [McpServerResource(UriTemplate = "windows365://cloudpcs", Name = "Cloud PCs List", MimeType = "application/json")]
    [Description("Complete list of Windows 365 Cloud PCs in the tenant")]
    public static async Task<string> CloudPCsList(CloudPCService cloudPCService)
    {
        var cloudPCs = await cloudPCService.GetCloudPCsAsync();
        var result = new
        {
            timestamp = DateTime.UtcNow,
            count = cloudPCs.Count(),
            cloudPCs = cloudPCs.Select(pc => new
            {
                id = pc.Id,
                displayName = pc.DisplayName,
                userPrincipalName = pc.UserPrincipalName,
                provisioningType = pc.ProvisioningType,
                servicePlanName = pc.ServicePlanName,
                lastModifiedDateTime = pc.LastModifiedDateTime,
                managedDeviceName = pc.ManagedDeviceName
            })
        };
        
        return JsonSerializer.Serialize(result, new JsonSerializerOptions { WriteIndented = true });
    }

    [McpServerResource(UriTemplate = "windows365://cloudpc/{id}", Name = "Cloud PC Details")]
    [Description("Detailed information about a specific Cloud PC")]
    public static async Task<ResourceContents> CloudPCDetails(
        RequestContext<ReadResourceRequestParams> requestContext, 
        CloudPCService cloudPCService,
        string id)
    {
        try
        {
            var cloudPC = await cloudPCService.GetCloudPCAsync(id);
            
            if (cloudPC == null)
            {
                throw new NotSupportedException($"Cloud PC not found: {id}");
            }

            var result = new
            {
                cloudPC = new
                {
                    id = cloudPC.Id,
                    displayName = cloudPC.DisplayName,
                    userPrincipalName = cloudPC.UserPrincipalName,
                    provisioningType = cloudPC.ProvisioningType,
                    provisioningPolicyName = cloudPC.ProvisioningPolicyName,
                    servicePlanName = cloudPC.ServicePlanName,
                    servicePlanId = cloudPC.ServicePlanId,
                    managedDeviceName = cloudPC.ManagedDeviceName,
                    managedDeviceId = cloudPC.ManagedDeviceId,
                    aadDeviceId = cloudPC.AadDeviceId,
                    lastModifiedDateTime = cloudPC.LastModifiedDateTime,
                    gracePeriodEndDateTime = cloudPC.GracePeriodEndDateTime,
                    imageDisplayName = cloudPC.ImageDisplayName,
                    onPremisesConnectionName = cloudPC.OnPremisesConnectionName,
                    provisioningPolicyId = cloudPC.ProvisioningPolicyId
                },
                retrievedAt = DateTime.UtcNow
            };

            return new TextResourceContents
            {
                Text = JsonSerializer.Serialize(result, new JsonSerializerOptions { WriteIndented = true }),
                MimeType = "application/json",
                Uri = requestContext.Params?.Uri
            };
        }
        catch (Exception ex)
        {
            return new TextResourceContents
            {
                Text = JsonSerializer.Serialize(new { error = ex.Message, cloudPcId = id }, new JsonSerializerOptions { WriteIndented = true }),
                MimeType = "application/json",
                Uri = requestContext.Params?.Uri
            };
        }
    }

    [McpServerResource(UriTemplate = "windows365://provisioning-policies", Name = "Provisioning Policies", MimeType = "application/json")]
    [Description("List of Windows 365 provisioning policies")]
    public static async Task<string> ProvisioningPoliciesList(ProvisioningPolicyService policyService)
    {
        var policies = await policyService.GetProvisioningPoliciesAsync();
        var result = new
        {
            timestamp = DateTime.UtcNow,
            count = policies.Count(),
            policies = policies.Select(policy => new
            {
                id = policy.Id,
                displayName = policy.DisplayName,
                description = policy.Description,
                provisioningType = policy.ProvisioningType?.ToString(),
                imageDisplayName = policy.ImageDisplayName,
                gracePeriodInHours = policy.GracePeriodInHours,
                localAdminEnabled = policy.LocalAdminEnabled
            })
        };

        return JsonSerializer.Serialize(result, new JsonSerializerOptions { WriteIndented = true });
    }

    [McpServerResource(UriTemplate = "windows365://licenses", Name = "License Information", MimeType = "application/json")]
    [Description("Windows 365 license availability and usage information")]
    public static async Task<string> LicenseInformation(LicenseService licenseService)
    {
        var licenses = await licenseService.GetAllWindows365LicensesAsync();
        var result = new
        {
            timestamp = DateTime.UtcNow,
            count = licenses.Count(),
            licenses = licenses.Select(license => new
            {
                skuId = license.SkuId,
                skuPartNumber = license.SkuPartNumber,
                capabilityStatus = license.CapabilityStatus,
                consumedUnits = license.ConsumedUnits,
                prepaidUnits = license.PrepaidUnits
            })
        };

        return JsonSerializer.Serialize(result, new JsonSerializerOptions { WriteIndented = true });
    }

    [McpServerResource(UriTemplate = "windows365://user-licenses/{userId}", Name = "User License Details")]
    [Description("License details for a specific user")]
    public static async Task<ResourceContents> UserLicenseDetails(
        RequestContext<ReadResourceRequestParams> requestContext,
        LicenseService licenseService,
        string userId)
    {
        try
        {
            var userLicenses = await licenseService.CheckUserLicensesAsync(userId);
            var result = new
            {
                userId = userId,
                licenses = userLicenses.Select(license => new
                {
                    skuId = license.SkuId,
                    skuPartNumber = license.SkuPartNumber,
                    capabilityStatus = license.CapabilityStatus,
                    servicePlans = license.ServicePlans
                }),
                retrievedAt = DateTime.UtcNow
            };

            return new TextResourceContents
            {
                Text = JsonSerializer.Serialize(result, new JsonSerializerOptions { WriteIndented = true }),
                MimeType = "application/json", 
                Uri = requestContext.Params?.Uri
            };
        }
        catch (Exception ex)
        {
            return new TextResourceContents
            {
                Text = JsonSerializer.Serialize(new { error = ex.Message, userId = userId }, new JsonSerializerOptions { WriteIndented = true }),
                MimeType = "application/json",
                Uri = requestContext.Params?.Uri
            };
        }
    }

    [McpServerResource(UriTemplate = "windows365://groups", Name = "Groups List", MimeType = "application/json")]
    [Description("List of Entra ID groups in the tenant")]
    public static async Task<string> GroupsList(UserService userService)
    {
        var response = await userService.GetGroupsAsync();
        var result = new
        {
            timestamp = DateTime.UtcNow,
            count = response?.Value?.Count ?? 0,
            groups = response?.Value?.Select(group => new
            {
                id = group.Id,
                displayName = group.DisplayName,
                description = group.Description,
                groupTypes = group.GroupTypes,
                mailEnabled = group.MailEnabled,
                securityEnabled = group.SecurityEnabled
            }) ?? Enumerable.Empty<object>()
        };

        return JsonSerializer.Serialize(result, new JsonSerializerOptions { WriteIndented = true });
    }

    [McpServerResource(UriTemplate = "windows365://tenant-summary", Name = "Tenant Summary", MimeType = "application/json")]
    [Description("Windows 365 tenant overview and summary statistics")]
    public static string TenantSummary()
    {
        var summary = new
        {
            tenantInfo = new
            {
                service = "Windows 365 Cloud PC",
                apiVersion = "v1.0",
                mcpServer = "Windows365.Mcp.Server",
                generatedAt = DateTime.UtcNow
            },
            availableCapabilities = new[]
            {
                "Cloud PC Discovery and Management",
                "Provisioning Policy Management",
                "License Management and Assignment",
                "Troubleshooting and Diagnostics"
            },
            endpoints = new
            {
                cloudPCs = "windows365://cloudpcs",
                policies = "windows365://provisioning-policies",
                licenses = "windows365://licenses",
                groups = "windows365://groups"
            }
        };

        return JsonSerializer.Serialize(summary, new JsonSerializerOptions { WriteIndented = true });
    }
}