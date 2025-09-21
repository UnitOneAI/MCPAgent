using Windows365.Mcp.Server.Services;

namespace Windows365.Mcp.Server.Tools;

[McpServerToolType]
public static class CloudPCDiscoveryMcpTool
{
    [McpServerTool, Description("Discover and list Cloud PCs in the Windows 365 tenant")]
    public static async Task<string> DiscoverCloudPCs(
        CloudPCService cloudPCService,
        [Description("Search filter to apply (optional)")] string? filter = null,
        [Description("Maximum number of Cloud PCs to return (1-999, default: 50)")] int top = 50)
    {
        try
        {
            var cloudPCs = await cloudPCService.GetCloudPCsAsync(filter, top);
            
            var result = new
            {
                success = true,
                count = cloudPCs.Count(),
                cloudPCs = cloudPCs.Select(pc => new
                {
                    id = pc.Id,
                    displayName = pc.DisplayName,
                    provisioningType = pc.ProvisioningType,
                    provisioningPolicyName = pc.ProvisioningPolicyName,
                    userPrincipalName = pc.UserPrincipalName,
                    lastModifiedDateTime = pc.LastModifiedDateTime,
                    managedDeviceName = pc.ManagedDeviceName,
                    managedDeviceId = pc.ManagedDeviceId,
                    aadDeviceId = pc.AadDeviceId,
                    servicePlanName = pc.ServicePlanName,
                    servicePlanId = pc.ServicePlanId,
                    imageDisplayName = pc.ImageDisplayName,
                    gracePeriodEndDateTime = pc.GracePeriodEndDateTime
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
                type = "CloudPC Discovery Error"
            };
            
            return JsonSerializer.Serialize(errorResult, new JsonSerializerOptions { WriteIndented = true });
        }
    }

    [McpServerTool, Description("Get detailed information about a specific Cloud PC")]
    public static async Task<string> GetCloudPCDetails(
        CloudPCService cloudPCService,
        [Description("The ID of the Cloud PC to get details for")] string cloudPcId)
    {
        try
        {
            var cloudPC = await cloudPCService.GetCloudPCAsync(cloudPcId);
            
            if (cloudPC == null)
            {
                var notFoundResult = new
                {
                    success = false,
                    error = $"Cloud PC with ID '{cloudPcId}' not found",
                    type = "Not Found Error"
                };
                
                return JsonSerializer.Serialize(notFoundResult, new JsonSerializerOptions { WriteIndented = true });
            }

            var result = new
            {
                success = true,
                cloudPC = new
                {
                    id = cloudPC.Id,
                    displayName = cloudPC.DisplayName,
                    provisioningType = cloudPC.ProvisioningType,
                    provisioningPolicyName = cloudPC.ProvisioningPolicyName,
                    userPrincipalName = cloudPC.UserPrincipalName,
                    lastModifiedDateTime = cloudPC.LastModifiedDateTime,
                    managedDeviceName = cloudPC.ManagedDeviceName,
                    managedDeviceId = cloudPC.ManagedDeviceId,
                    aadDeviceId = cloudPC.AadDeviceId,
                    gracePeriodEndDateTime = cloudPC.GracePeriodEndDateTime,
                    imageDisplayName = cloudPC.ImageDisplayName,
                    servicePlanName = cloudPC.ServicePlanName,
                    servicePlanId = cloudPC.ServicePlanId,
                    onPremisesConnectionName = cloudPC.OnPremisesConnectionName,
                    provisioningPolicyId = cloudPC.ProvisioningPolicyId
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
                type = "CloudPC Details Error"
            };
            
            return JsonSerializer.Serialize(errorResult, new JsonSerializerOptions { WriteIndented = true });
        }
    }
}