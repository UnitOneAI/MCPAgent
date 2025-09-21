using System.Text.Json.Serialization;

namespace Windows365.Mcp.Server.Models;

public class CloudPC
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;

    [JsonPropertyName("displayName")]
    public string DisplayName { get; set; } = string.Empty;

    [JsonPropertyName("imageDisplayName")]
    public string ImageDisplayName { get; set; } = string.Empty;

    [JsonPropertyName("provisioningPolicyId")]
    public string ProvisioningPolicyId { get; set; } = string.Empty;

    [JsonPropertyName("provisioningPolicyName")]
    public string ProvisioningPolicyName { get; set; } = string.Empty;

    [JsonPropertyName("onPremisesConnectionName")]
    public string OnPremisesConnectionName { get; set; } = string.Empty;

    [JsonPropertyName("servicePlanId")]
    public string ServicePlanId { get; set; } = string.Empty;

    [JsonPropertyName("servicePlanName")]
    public string ServicePlanName { get; set; } = string.Empty;

    [JsonPropertyName("userPrincipalName")]
    public string UserPrincipalName { get; set; } = string.Empty;

    [JsonPropertyName("lastModifiedDateTime")]
    public DateTimeOffset? LastModifiedDateTime { get; set; }

    [JsonPropertyName("managedDeviceId")]
    public string ManagedDeviceId { get; set; } = string.Empty;

    [JsonPropertyName("managedDeviceName")]
    public string ManagedDeviceName { get; set; } = string.Empty;

    [JsonPropertyName("aadDeviceId")]
    public string AadDeviceId { get; set; } = string.Empty;

    [JsonPropertyName("gracePeriodEndDateTime")]
    public DateTimeOffset? GracePeriodEndDateTime { get; set; }

    [JsonPropertyName("provisioningType")]
    public string ProvisioningType { get; set; } = string.Empty;
}