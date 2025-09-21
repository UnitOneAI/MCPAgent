using Microsoft.Graph;
using Microsoft.Graph.Models;
using Windows365.Mcp.Server.Models;

namespace Windows365.Mcp.Server.Services;

public class CloudPCService
{
    private readonly GraphServiceClient _graphClient;
    private readonly ILogger<CloudPCService> _logger;

    public CloudPCService(GraphServiceClient graphClient, ILogger<CloudPCService> logger)
    {
        _graphClient = graphClient;
        _logger = logger;
    }

    public async Task<IEnumerable<Windows365.Mcp.Server.Models.CloudPC>> GetCloudPCsAsync(string? filter = null, int top = 50)
    {
        try
        {
            var response = await _graphClient.DeviceManagement.VirtualEndpoint.CloudPCs.GetAsync(requestConfiguration =>
            {
                requestConfiguration.QueryParameters.Top = top;
                if (!string.IsNullOrEmpty(filter))
                {
                    requestConfiguration.QueryParameters.Filter = filter;
                }
            });

            return response?.Value?.Select(MapToCloudPC) ?? Enumerable.Empty<Windows365.Mcp.Server.Models.CloudPC>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting Cloud PCs with filter: {Filter}", filter);
            throw;
        }
    }

    public async Task<Windows365.Mcp.Server.Models.CloudPC?> GetCloudPCAsync(string cloudPCId)
    {
        try
        {
            var cloudPC = await _graphClient.DeviceManagement.VirtualEndpoint.CloudPCs[cloudPCId].GetAsync();
            return cloudPC != null ? MapToCloudPC(cloudPC) : null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting Cloud PC: {CloudPCId}", cloudPCId);
            throw;
        }
    }

    public async Task<IEnumerable<Windows365.Mcp.Server.Models.CloudPC>> GetUserCloudPCsAsync(string userId)
    {
        try
        {
            var filter = $"userPrincipalName eq '{userId}'";
            return await GetCloudPCsAsync(filter);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting Cloud PCs for user: {UserId}", userId);
            throw;
        }
    }

    public async Task RebootCloudPCAsync(string cloudPCId)
    {
        try
        {
            // POST /deviceManagement/virtualEndpoint/cloudPCs/{cloudPCId}/reboot
            await _graphClient.DeviceManagement.VirtualEndpoint.CloudPCs[cloudPCId].Reboot.PostAsync();
            _logger.LogInformation("Reboot initiated for Cloud PC: {CloudPCId}", cloudPCId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error rebooting Cloud PC: {CloudPCId}", cloudPCId);
            throw;
        }
    }

    public async Task<object> GetCloudPCStatusAsync(string cloudPCId)
    {
        try
        {
            // Get the CloudPC which includes status information
            var cloudPC = await GetCloudPCAsync(cloudPCId);
            if (cloudPC == null)
            {
                throw new InvalidOperationException($"Cloud PC {cloudPCId} not found");
            }

            return new
            {
                Id = cloudPC.Id,
                ProvisioningType = cloudPC.ProvisioningType,
                LastModifiedDateTime = cloudPC.LastModifiedDateTime,
                GracePeriodEndDateTime = cloudPC.GracePeriodEndDateTime
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting Cloud PC status: {CloudPCId}", cloudPCId);
            throw;
        }
    }

    // Reprovision API is only available in Microsoft Graph beta version
    // public async Task ReprovisionCloudPCAsync(string cloudPCId) - REMOVED (beta only)

    public async Task<ToolResponse<object>> EndGracePeriodAsync(string cloudPCId)
    {
        try
        {
            // POST /deviceManagement/virtualEndpoint/cloudPCs/{cloudPCId}/endGracePeriod
            await _graphClient.DeviceManagement.VirtualEndpoint.CloudPCs[cloudPCId].EndGracePeriod.PostAsync();
            _logger.LogInformation("Grace period ended for Cloud PC: {CloudPCId}", cloudPCId);

            return new ToolResponse<object>
            {
                Success = true,
                Message = $"Grace period ended successfully for Cloud PC {cloudPCId}"
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error ending grace period for Cloud PC: {CloudPCId}", cloudPCId);
            return new ToolResponse<object>
            {
                Success = false,
                Error = $"Error ending grace period: {ex.Message}"
            };
        }
    }

    private static Windows365.Mcp.Server.Models.CloudPC MapToCloudPC(Microsoft.Graph.Models.CloudPC cloudPC)
    {
        return new Windows365.Mcp.Server.Models.CloudPC
        {
            Id = cloudPC.Id ?? string.Empty,
            DisplayName = cloudPC.DisplayName ?? string.Empty,
            ImageDisplayName = cloudPC.ImageDisplayName ?? string.Empty,
            ProvisioningPolicyId = cloudPC.ProvisioningPolicyId ?? string.Empty,
            ProvisioningPolicyName = cloudPC.ProvisioningPolicyName ?? string.Empty,
            OnPremisesConnectionName = cloudPC.OnPremisesConnectionName ?? string.Empty,
            ServicePlanId = cloudPC.ServicePlanId ?? string.Empty,
            ServicePlanName = cloudPC.ServicePlanName ?? string.Empty,
            UserPrincipalName = cloudPC.UserPrincipalName ?? string.Empty,
            LastModifiedDateTime = cloudPC.LastModifiedDateTime,
            ManagedDeviceId = cloudPC.ManagedDeviceId ?? string.Empty,
            ManagedDeviceName = cloudPC.ManagedDeviceName ?? string.Empty,
            AadDeviceId = cloudPC.AadDeviceId ?? string.Empty,
            GracePeriodEndDateTime = cloudPC.GracePeriodEndDateTime,
            ProvisioningType = cloudPC.ProvisioningType?.ToString() ?? string.Empty
        };
    }
}