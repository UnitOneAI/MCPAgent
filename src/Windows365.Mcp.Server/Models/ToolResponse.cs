using System.Text.Json.Serialization;

namespace Windows365.Mcp.Server.Models;

public class ToolResponse<T>
{
    [JsonPropertyName("success")]
    public bool Success { get; set; }

    [JsonPropertyName("data")]
    public T? Data { get; set; }

    [JsonPropertyName("message")]
    public string? Message { get; set; }

    [JsonPropertyName("error")]
    public string? Error { get; set; }

    [JsonPropertyName("timestamp")]
    public DateTimeOffset Timestamp { get; set; } = DateTimeOffset.UtcNow;

    [JsonPropertyName("requestId")]
    public string RequestId { get; set; } = Guid.NewGuid().ToString();
}

public class LicenseInfo
{
    [JsonPropertyName("skuId")]
    public string SkuId { get; set; } = string.Empty;

    [JsonPropertyName("skuPartNumber")]
    public string SkuPartNumber { get; set; } = string.Empty;

    [JsonPropertyName("servicePlans")]
    public List<ServicePlan> ServicePlans { get; set; } = new();


    [JsonPropertyName("capabilityStatus")]
    public string CapabilityStatus { get; set; } = string.Empty;

    [JsonPropertyName("consumedUnits")]
    public int ConsumedUnits { get; set; }

    [JsonPropertyName("prepaidUnits")]
    public PrepaidUnits PrepaidUnits { get; set; } = new();
}

public class ServicePlan
{
    [JsonPropertyName("servicePlanId")]
    public string ServicePlanId { get; set; } = string.Empty;

    [JsonPropertyName("servicePlanName")]
    public string ServicePlanName { get; set; } = string.Empty;

    [JsonPropertyName("provisioningStatus")]
    public string ProvisioningStatus { get; set; } = string.Empty;

    [JsonPropertyName("appliesTo")]
    public string AppliesTo { get; set; } = string.Empty;
}

public class PrepaidUnits
{
    [JsonPropertyName("enabled")]
    public int Enabled { get; set; }

    [JsonPropertyName("suspended")]
    public int Suspended { get; set; }

    [JsonPropertyName("warning")]
    public int Warning { get; set; }
}

public class UserInfo
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;

    [JsonPropertyName("userPrincipalName")]
    public string UserPrincipalName { get; set; } = string.Empty;

    [JsonPropertyName("displayName")]
    public string DisplayName { get; set; } = string.Empty;

    [JsonPropertyName("givenName")]
    public string GivenName { get; set; } = string.Empty;

    [JsonPropertyName("surname")]
    public string Surname { get; set; } = string.Empty;

    [JsonPropertyName("mail")]
    public string Mail { get; set; } = string.Empty;

    [JsonPropertyName("jobTitle")]
    public string JobTitle { get; set; } = string.Empty;

    [JsonPropertyName("department")]
    public string Department { get; set; } = string.Empty;

    [JsonPropertyName("accountEnabled")]
    public bool AccountEnabled { get; set; }

    [JsonPropertyName("createdDateTime")]
    public DateTimeOffset? CreatedDateTime { get; set; }
}

public class GroupInfo
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;

    [JsonPropertyName("displayName")]
    public string DisplayName { get; set; } = string.Empty;

    [JsonPropertyName("description")]
    public string Description { get; set; } = string.Empty;

    [JsonPropertyName("groupTypes")]
    public List<string> GroupTypes { get; set; } = new();

    [JsonPropertyName("securityEnabled")]
    public bool SecurityEnabled { get; set; }

    [JsonPropertyName("mailEnabled")]
    public bool MailEnabled { get; set; }

    [JsonPropertyName("createdDateTime")]
    public DateTimeOffset? CreatedDateTime { get; set; }
}