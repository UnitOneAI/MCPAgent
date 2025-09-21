using Windows365.Mcp.Server.Services;

namespace Windows365.Mcp.Server.Tools;

[McpServerToolType]
public static class UserMcpTool
{
    [McpServerTool, Description("Search and list users in the Entra ID tenant")]
    public static async Task<string> SearchUsers(
        UserService userService,
        [Description("Optional search term to filter users by display name or UPN")] string? search = null,
        [Description("Maximum number of users to return (default: 50)")] int top = 50)
    {
        try
        {
            var response = await userService.GetUsersAsync(top, search);

            var result = new
            {
                success = true,
                count = response?.Value?.Count ?? 0,
                users = response?.Value?.Select(user => new
                {
                    id = user.Id,
                    userPrincipalName = user.UserPrincipalName,
                    displayName = user.DisplayName,
                    mail = user.Mail,
                    accountEnabled = user.AccountEnabled
                }) ?? Enumerable.Empty<object>()
            };

            return JsonSerializer.Serialize(result, new JsonSerializerOptions { WriteIndented = true });
        }
        catch (Exception ex)
        {
            var errorResult = new
            {
                success = false,
                error = ex.Message,
                type = "User Search Error"
            };

            return JsonSerializer.Serialize(errorResult, new JsonSerializerOptions { WriteIndented = true });
        }
    }

    [McpServerTool, Description("Get detailed information about a specific user")]
    public static async Task<string> GetUserDetails(
        UserService userService,
        [Description("The user ID or user principal name")] string userId)
    {
        try
        {
            var user = await userService.GetUserAsync(userId);

            if (user == null)
            {
                var notFoundResult = new
                {
                    success = false,
                    error = "User not found",
                    userId = userId
                };

                return JsonSerializer.Serialize(notFoundResult, new JsonSerializerOptions { WriteIndented = true });
            }

            var result = new
            {
                success = true,
                user = new
                {
                    id = user.Id,
                    userPrincipalName = user.UserPrincipalName,
                    displayName = user.DisplayName,
                    mail = user.Mail,
                    accountEnabled = user.AccountEnabled,
                    assignedLicenses = user.AssignedLicenses?.Select(license => new
                    {
                        skuId = license.SkuId,
                        disabledPlans = license.DisabledPlans
                    }) ?? Enumerable.Empty<object>()
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
                type = "User Details Error",
                userId = userId
            };

            return JsonSerializer.Serialize(errorResult, new JsonSerializerOptions { WriteIndented = true });
        }
    }

    [McpServerTool, Description("Search and list groups in the Entra ID tenant")]
    public static async Task<string> SearchGroups(
        UserService userService,
        [Description("Optional search term to filter groups by display name")] string? search = null,
        [Description("Maximum number of groups to return (default: 50)")] int top = 50)
    {
        try
        {
            var response = await userService.GetGroupsAsync(top, search);

            var result = new
            {
                success = true,
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
        catch (Exception ex)
        {
            var errorResult = new
            {
                success = false,
                error = ex.Message,
                type = "Group Search Error"
            };

            return JsonSerializer.Serialize(errorResult, new JsonSerializerOptions { WriteIndented = true });
        }
    }

}