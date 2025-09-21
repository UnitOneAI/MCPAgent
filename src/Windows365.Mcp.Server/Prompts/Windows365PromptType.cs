using ChatMessage = Microsoft.Extensions.AI.ChatMessage;
using ChatRole = Microsoft.Extensions.AI.ChatRole;

namespace Windows365.Mcp.Server.Prompts;

[McpServerPromptType]
public class Windows365PromptType
{
    [McpServerPrompt(Name = "cloud_pc_troubleshoot"), Description("Generate troubleshooting steps for Cloud PC issues")]
    public static IEnumerable<ChatMessage> CloudPCTroubleshootPrompt(
        [Description("The Cloud PC issue description")] string issue,
        [Description("The Cloud PC ID (optional)")] string? cloudPcId = null,
        [Description("User principal name (optional)")] string? userPrincipalName = null)
    {
        var messages = new List<ChatMessage>
        {
            new ChatMessage(ChatRole.System, "You are a Windows 365 Cloud PC expert. Provide step-by-step troubleshooting guidance."),
            new ChatMessage(ChatRole.User, $"I need help troubleshooting this Cloud PC issue: {issue}")
        };

        if (!string.IsNullOrEmpty(cloudPcId))
        {
            messages.Add(new ChatMessage(ChatRole.User, $"Cloud PC ID: {cloudPcId}"));
        }

        if (!string.IsNullOrEmpty(userPrincipalName))
        {
            messages.Add(new ChatMessage(ChatRole.User, $"User: {userPrincipalName}"));
        }

        messages.Add(new ChatMessage(ChatRole.User, "Please use available tools like DiscoverCloudPCs and GetCloudPCDetails to gather diagnostic information, then provide specific troubleshooting steps and common solutions for this type of issue."));

        return messages;
    }

    [McpServerPrompt(Name = "provisioning_policy_analysis"), Description("Analyze Windows 365 provisioning policies based on current policy data")]
    public static IEnumerable<ChatMessage> ProvisioningPolicyAnalysisPrompt(
        [Description("The provisioning policy configuration to analyze")] string policyConfig,
        [Description("Analysis focus area (configuration, settings, compliance)")] string focusArea = "configuration")
    {
        return [
            new ChatMessage(ChatRole.System, "You are a Windows 365 policy analyst. Review policy configurations based on available data and provide insights."),
            new ChatMessage(ChatRole.User, $"Please analyze this Windows 365 provisioning policy configuration:\n\n{policyConfig}"),
            new ChatMessage(ChatRole.User, $"Focus your analysis on: {focusArea}"),
            new ChatMessage(ChatRole.User, "Use GetProvisioningPolicies and GetProvisioningPolicyDetails tools to gather complete policy information, then provide insights about configuration, potential issues, and best practices.")
        ];
    }

    [McpServerPrompt(Name = "license_optimization"), Description("Generate license optimization recommendations")]
    public static IEnumerable<ChatMessage> LicenseOptimizationPrompt(
        [Description("Current license usage data")] string licenseData,
        [Description("Organization size")] int organizationSize)
    {
        return [
            new ChatMessage(ChatRole.System, "You are a Windows 365 licensing specialist. Provide cost-effective licensing recommendations."),
            new ChatMessage(ChatRole.User, $"Current license usage:\n{licenseData}"),
            new ChatMessage(ChatRole.User, $"Organization size: {organizationSize} users"),
            new ChatMessage(ChatRole.User, "Please use GetWindows365Licenses to analyze current license availability, CheckUserLicenses to review assignments, and provide recommendations for optimization using AssignLicense/UnassignLicense tools as needed. When recommending license unassignment, ask the user if they also want to immediately end the grace period using EndGracePeriod to reclaim Cloud PC resources faster.")
        ];
    }

    [McpServerPrompt(Name = "deployment_planning"), Description("Generate Windows 365 deployment planning guidance based on current inventory and licensing")]
    public static IEnumerable<ChatMessage> DeploymentPlanningPrompt(
        [Description("Target user count")] int userCount,
        [Description("Deployment timeline in weeks")] int timelineWeeks,
        [Description("Industry or use case")] string industry = "general")
    {
        return [
            new ChatMessage(ChatRole.System, "You are a Windows 365 deployment specialist. Create deployment plans based on existing inventory analysis and license management capabilities. Focus on capacity assessment and license planning."),
            new ChatMessage(ChatRole.User, $"I need to plan a Windows 365 deployment for {userCount} users"),
            new ChatMessage(ChatRole.User, $"Timeline: {timelineWeeks} weeks"),
            new ChatMessage(ChatRole.User, $"Industry/Use case: {industry}"),
            new ChatMessage(ChatRole.User, "Please use available tools to analyze current Cloud PC inventory (DiscoverCloudPCs), available licenses (GetWindows365Licenses), and existing provisioning policies (GetProvisioningPolicies) to create a deployment plan. Focus on license allocation, capacity planning, and preparation steps.")
        ];
    }

    [McpServerPrompt(Name = "user_provisioning"), Description("Guide Cloud PC provisioning workflow for new users using existing policies")]
    public static IEnumerable<ChatMessage> UserProvisioningPrompt(
        [Description("User principal name or ID to provision Cloud PC for")] string userId,
        [Description("Preferred provisioning policy name (optional)")] string? preferredPolicy = null)
    {
        var messages = new List<ChatMessage>
        {
            new ChatMessage(ChatRole.System, "You are a Windows 365 provisioning specialist. Guide users through Cloud PC provisioning using existing infrastructure and available tools."),
            new ChatMessage(ChatRole.User, $"I need to provision a Cloud PC for user: {userId}")
        };

        if (!string.IsNullOrEmpty(preferredPolicy))
        {
            messages.Add(new ChatMessage(ChatRole.User, $"Preferred policy: {preferredPolicy}"));
        }

        messages.Add(new ChatMessage(ChatRole.User, "Please use GetProvisioningPolicies to show available policies, SearchGroups to find groups with policy assignments, AssignLicense to assign Windows 365 license, and provide manual instructions for adding the user to the appropriate group (since group membership modification requires admin portal access)."));

        return messages;
    }

    [McpServerPrompt(Name = "security_assessment"), Description("Generate Windows 365 configuration review based on available inventory data")]
    public static string SecurityAssessmentPrompt()
    {
        return "Use available tools to analyze the current Windows 365 environment: DiscoverCloudPCs for Cloud PC inventory, " +
               "GetProvisioningPolicies for policy configurations, GetWindows365Licenses for licensing data, and SearchUsers/SearchGroups for user access patterns. " +
               "Focus on configuration analysis, license allocation patterns, and policy settings gathered through these tools. " +
               "Provide insights about current configuration patterns and general best practices.";
    }
}