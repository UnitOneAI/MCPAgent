using System.Text.Json.Serialization;

namespace Windows365.Mcp.Server.Models;

public class ProvisioningResult
{
    [JsonPropertyName("userId")]
    public string UserId { get; set; } = string.Empty;

    [JsonPropertyName("success")]
    public bool Success { get; set; }

    [JsonPropertyName("message")]
    public string Message { get; set; } = string.Empty;

    [JsonPropertyName("steps")]
    public List<ProvisioningStep> Steps { get; set; } = new();

    [JsonPropertyName("timestamp")]
    public DateTimeOffset Timestamp { get; set; } = DateTimeOffset.UtcNow;

    [JsonPropertyName("estimatedCompletionTime")]
    public string EstimatedCompletionTime { get; set; } = "30-60 minutes";

    [JsonPropertyName("licenseAssigned")]
    public bool LicenseAssigned { get; set; }

    [JsonPropertyName("groupMembershipAdded")]
    public bool GroupMembershipAdded { get; set; }

    [JsonPropertyName("groupId")]
    public string? GroupId { get; set; }

    [JsonPropertyName("policyId")]
    public string? PolicyId { get; set; }
}

public class ProvisioningStep
{
    [JsonPropertyName("stepName")]
    public string StepName { get; set; } = string.Empty;

    [JsonPropertyName("status")]
    public ProvisioningStepStatus Status { get; set; }

    [JsonPropertyName("message")]
    public string Message { get; set; } = string.Empty;

    [JsonPropertyName("errorDetails")]
    public string? ErrorDetails { get; set; }

    [JsonPropertyName("timestamp")]
    public DateTimeOffset Timestamp { get; set; } = DateTimeOffset.UtcNow;

    [JsonPropertyName("completedDateTime")]
    public DateTimeOffset? CompletedDateTime { get; set; }
}

public enum ProvisioningStepStatus
{
    [JsonPropertyName("pending")]
    Pending,
    
    [JsonPropertyName("in_progress")]
    InProgress,
    
    [JsonPropertyName("completed")]
    Completed,
    
    [JsonPropertyName("failed")]
    Failed,
    
    [JsonPropertyName("skipped")]
    Skipped
}