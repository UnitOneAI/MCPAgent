using Microsoft.Graph;
using Microsoft.Graph.Models;
using Windows365.Mcp.Server.Models;

namespace Windows365.Mcp.Server.Services;

public class ProvisioningService
{
    private readonly GraphServiceClient _graphClient;
    private readonly LicenseService _licenseService;
    private readonly ILogger<ProvisioningService> _logger;

    public ProvisioningService(GraphServiceClient graphClient, LicenseService licenseService, ILogger<ProvisioningService> logger)
    {
        _graphClient = graphClient;
        _licenseService = licenseService;
        _logger = logger;
    }

    public async Task<Windows365.Mcp.Server.Models.ProvisioningResult> ProvisionForUserAsync(string userId, string? provisioningGroupId = null, string? licenseSkuId = null)
    {
        var result = new Windows365.Mcp.Server.Models.ProvisioningResult
        {
            UserId = userId,
            Steps = new List<Windows365.Mcp.Server.Models.ProvisioningStep>()
        };

        try
        {
            // Step 1: Verify or assign Windows 365 license
            var licenseStep = new Windows365.Mcp.Server.Models.ProvisioningStep
            {
                StepName = "License Assignment",
                Status = Windows365.Mcp.Server.Models.ProvisioningStepStatus.InProgress
            };
            result.Steps.Add(licenseStep);

            var hasLicense = await _licenseService.HasWindows365LicenseAsync(userId);
            if (!hasLicense)
            {
                var licenseResult = await _licenseService.AssignWindows365LicenseAsync(userId, licenseSkuId);
                if (!licenseResult.Success)
                {
                    licenseStep.Status = Windows365.Mcp.Server.Models.ProvisioningStepStatus.Failed;
                    licenseStep.ErrorDetails = licenseResult.Error;
                    result.Success = false;
                    result.Message = $"Failed to assign Windows 365 license: {licenseResult.Error}";
                    return result;
                }
                result.LicenseAssigned = true;
                _logger.LogInformation("Assigned Windows 365 license to user {UserId}", userId);
            }

            licenseStep.Status = Windows365.Mcp.Server.Models.ProvisioningStepStatus.Completed;
            licenseStep.CompletedDateTime = DateTimeOffset.UtcNow;
            licenseStep.Message = hasLicense ? "License already assigned" : "License assigned successfully";

            // Step 2: Add to provisioning group
            var groupStep = new Windows365.Mcp.Server.Models.ProvisioningStep
            {
                StepName = "Group Membership",
                Status = Windows365.Mcp.Server.Models.ProvisioningStepStatus.InProgress
            };
            result.Steps.Add(groupStep);

            if (string.IsNullOrEmpty(provisioningGroupId))
            {
                provisioningGroupId = await GetDefaultProvisioningGroupIdAsync();
            }

            if (string.IsNullOrEmpty(provisioningGroupId))
            {
                groupStep.Status = Windows365.Mcp.Server.Models.ProvisioningStepStatus.Failed;
                groupStep.ErrorDetails = "No provisioning group ID provided and no default group found";
                result.Success = false;
                result.Message = "Failed to find provisioning group";
                return result;
            }

            await AddUserToGroupAsync(userId, provisioningGroupId);
            result.GroupMembershipAdded = true;
            result.GroupId = provisioningGroupId;
            _logger.LogInformation("Added user {UserId} to provisioning group {GroupId}", userId, provisioningGroupId);

            groupStep.Status = Windows365.Mcp.Server.Models.ProvisioningStepStatus.Completed;
            groupStep.CompletedDateTime = DateTimeOffset.UtcNow;
            groupStep.Message = "Added to provisioning group successfully";

            // Step 3: Verify provisioning policy exists
            var policyStep = new Windows365.Mcp.Server.Models.ProvisioningStep
            {
                StepName = "Policy Verification",
                Status = Windows365.Mcp.Server.Models.ProvisioningStepStatus.InProgress
            };
            result.Steps.Add(policyStep);

            var policy = await VerifyProvisioningPolicyAsync(provisioningGroupId);
            if (policy == null)
            {
                policyStep.Status = Windows365.Mcp.Server.Models.ProvisioningStepStatus.Failed;
                policyStep.ErrorDetails = $"No provisioning policy found for group {provisioningGroupId}";
                result.Success = false;
                result.Message = $"No provisioning policy found for group {provisioningGroupId}";
                return result;
            }

            result.PolicyId = policy.Id;
            policyStep.Status = Windows365.Mcp.Server.Models.ProvisioningStepStatus.Completed;
            policyStep.CompletedDateTime = DateTimeOffset.UtcNow;
            policyStep.Message = "Provisioning policy verified";

            // Step 4: Monitor provisioning (async process)
            var monitorStep = new Windows365.Mcp.Server.Models.ProvisioningStep
            {
                StepName = "Auto-Provisioning Initiated",
                Status = Windows365.Mcp.Server.Models.ProvisioningStepStatus.Completed,
                CompletedDateTime = DateTimeOffset.UtcNow,
                Message = "Cloud PC auto-provisioning has been initiated"
            };
            result.Steps.Add(monitorStep);

            result.Success = true;
            result.Message = "Provisioning initiated successfully. Cloud PC will be available in 30-60 minutes.";

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error provisioning Cloud PC for user: {UserId}", userId);
            result.Success = false;
            result.Message = $"Error during provisioning: {ex.Message}";
            return result;
        }
    }

    private async Task<string?> GetDefaultProvisioningGroupIdAsync()
    {
        try
        {
            // Get all groups and find one with "CloudPC" or "Windows365" in the name
            var response = await _graphClient.Groups.GetAsync(requestConfiguration =>
            {
                requestConfiguration.QueryParameters.Filter = "displayName/contains('CloudPC') or displayName/contains('Windows365')";
                requestConfiguration.QueryParameters.Top = 10;
            });

            return response?.Value?.FirstOrDefault()?.Id;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Could not find default provisioning group");
            return null;
        }
    }

    private async Task AddUserToGroupAsync(string userId, string groupId)
    {
        try
        {
            var requestBody = new ReferenceCreate
            {
                OdataId = $"https://graph.microsoft.com/v1.0/directoryObjects/{userId}"
            };

            await _graphClient.Groups[groupId].Members.Ref.PostAsync(requestBody);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error adding user {UserId} to group {GroupId}", userId, groupId);
            throw;
        }
    }

    private async Task<CloudPcProvisioningPolicy?> VerifyProvisioningPolicyAsync(string groupId)
    {
        try
        {
            // Get provisioning policies and find one that targets the group
            var response = await _graphClient.DeviceManagement.VirtualEndpoint.ProvisioningPolicies.GetAsync();
            
            if (response?.Value == null) return null;

            foreach (var policy in response.Value)
            {
                // Check if this policy is assigned to the group
                var assignments = await _graphClient.DeviceManagement.VirtualEndpoint.ProvisioningPolicies[policy.Id].Assignments.GetAsync();
                
                if (assignments?.Value?.Any(a => a.Target is CloudPcManagementGroupAssignmentTarget groupTarget && groupTarget.GroupId == groupId) == true)
                {
                    return policy;
                }
            }

            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error verifying provisioning policy for group: {GroupId}", groupId);
            throw;
        }
    }
}