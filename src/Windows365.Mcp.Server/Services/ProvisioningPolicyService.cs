using Microsoft.Graph;
using Microsoft.Graph.Models;

namespace Windows365.Mcp.Server.Services;

public class ProvisioningPolicyService
{
    private readonly GraphServiceClient _graphServiceClient;

    public ProvisioningPolicyService(GraphServiceClient graphServiceClient)
    {
        _graphServiceClient = graphServiceClient;
    }

    public async Task<IEnumerable<CloudPcProvisioningPolicy>> GetProvisioningPoliciesAsync()
    {
        var policies = await _graphServiceClient.DeviceManagement.VirtualEndpoint.ProvisioningPolicies.GetAsync();
        return policies?.Value ?? new List<CloudPcProvisioningPolicy>();
    }

    public async Task<CloudPcProvisioningPolicy?> GetProvisioningPolicyAsync(string policyId, bool includeAssignments = false)
    {
        if (includeAssignments)
        {
            return await _graphServiceClient.DeviceManagement.VirtualEndpoint.ProvisioningPolicies[policyId].GetAsync(requestConfiguration =>
            {
                requestConfiguration.QueryParameters.Expand = new string[] { "assignments" };
            });
        }

        return await _graphServiceClient.DeviceManagement.VirtualEndpoint.ProvisioningPolicies[policyId].GetAsync();
    }

    public async Task<IEnumerable<CloudPcProvisioningPolicyAssignment>> GetProvisioningPolicyAssignmentsAsync(string policyId)
    {
        var assignments = await _graphServiceClient.DeviceManagement.VirtualEndpoint.ProvisioningPolicies[policyId].Assignments.GetAsync();
        return assignments?.Value ?? new List<CloudPcProvisioningPolicyAssignment>();
    }
}