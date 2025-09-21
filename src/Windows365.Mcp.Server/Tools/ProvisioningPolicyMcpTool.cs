using Windows365.Mcp.Server.Services;

namespace Windows365.Mcp.Server.Tools;

[McpServerToolType]
public static class ProvisioningPolicyMcpTool
{
    [McpServerTool, Description("Get all provisioning policies in the Windows 365 tenant")]
    public static async Task<string> GetProvisioningPolicies(ProvisioningPolicyService provisioningPolicyService)
    {
        try
        {
            var policies = await provisioningPolicyService.GetProvisioningPoliciesAsync();
            
            var result = new
            {
                success = true,
                count = policies.Count(),
                provisioningPolicies = policies.Select(policy => new
                {
                    id = policy.Id,
                    displayName = policy.DisplayName,
                    description = policy.Description,
                    provisioningType = policy.ProvisioningType?.ToString(),
                    imageType = policy.ImageType?.ToString(),
                    imageId = policy.ImageId,
                    imageDisplayName = policy.ImageDisplayName,
                    cloudPcGroupDisplayName = policy.CloudPcGroupDisplayName,
                    gracePeriodInHours = policy.GracePeriodInHours,
                    localAdminEnabled = policy.LocalAdminEnabled,
                    domainJoinConfigurations = policy.DomainJoinConfigurations
                })
            };

            return JsonSerializer.Serialize(result, new JsonSerializerOptions { WriteIndented = true });
        }
        catch (Exception ex)
        {
            var errorResult = new
            {
                success = false,
                error = ex.Message,
                type = "Provisioning Policy Error"
            };
            
            return JsonSerializer.Serialize(errorResult, new JsonSerializerOptions { WriteIndented = true });
        }
    }

    [McpServerTool, Description("Get detailed information about a specific provisioning policy with optional assignments")]
    public static async Task<string> GetProvisioningPolicyDetails(
        ProvisioningPolicyService provisioningPolicyService,
        [Description("The ID of the provisioning policy to get details for")] string policyId,
        [Description("Include assignments in the response (default: true)")] bool includeAssignments = true)
    {
        try
        {
            var policy = await provisioningPolicyService.GetProvisioningPolicyAsync(policyId, includeAssignments);
            
            if (policy == null)
            {
                var notFoundResult = new
                {
                    success = false,
                    error = $"Provisioning policy with ID '{policyId}' not found",
                    type = "Not Found Error"
                };
                
                return JsonSerializer.Serialize(notFoundResult, new JsonSerializerOptions { WriteIndented = true });
            }

            var result = new
            {
                success = true,
                provisioningPolicy = new
                {
                    id = policy.Id,
                    displayName = policy.DisplayName,
                    description = policy.Description,
                    provisioningType = policy.ProvisioningType?.ToString(),
                    imageType = policy.ImageType?.ToString(),
                    imageId = policy.ImageId,
                    imageDisplayName = policy.ImageDisplayName,
                    cloudPcGroupDisplayName = policy.CloudPcGroupDisplayName,
                    gracePeriodInHours = policy.GracePeriodInHours,
                    localAdminEnabled = policy.LocalAdminEnabled,
                    domainJoinConfigurations = policy.DomainJoinConfigurations,
                    enableSingleSignOn = policy.EnableSingleSignOn,
                    assignments = includeAssignments && policy.Assignments != null ?
                        policy.Assignments.Select(assignment => new
                        {
                            id = assignment.Id,
                            target = assignment.Target
                        }) : null
                }
            };

            return JsonSerializer.Serialize(result, new JsonSerializerOptions { WriteIndented = true });
        }
        catch (Exception ex)
        {
            var errorResult = new
            {
                success = false,
                error = ex.Message,
                type = "Provisioning Policy Details Error",
                policyId = policyId
            };
            
            return JsonSerializer.Serialize(errorResult, new JsonSerializerOptions { WriteIndented = true });
        }
    }

}