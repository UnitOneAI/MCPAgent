using Microsoft.Graph;
using Microsoft.Graph.Models;

namespace Windows365.Mcp.Server.Services;

public class UserService
{
    private readonly GraphServiceClient _graphClient;
    private readonly ILogger<UserService> _logger;

    public UserService(GraphServiceClient graphClient, ILogger<UserService> logger)
    {
        _graphClient = graphClient;
        _logger = logger;
    }

    public async Task<UserCollectionResponse?> GetUsersAsync(int top = 50, string? search = null)
    {
        try
        {
            var response = await _graphClient.Users.GetAsync(requestConfiguration =>
            {
                requestConfiguration.QueryParameters.Top = top;
                requestConfiguration.QueryParameters.Select = new[] { "id", "userPrincipalName", "displayName", "mail", "accountEnabled" };

                if (!string.IsNullOrEmpty(search))
                {
                    requestConfiguration.QueryParameters.Search = $"\"displayName:{search}\" OR \"userPrincipalName:{search}\"";
                    requestConfiguration.Headers.Add("ConsistencyLevel", "eventual");
                }
            });

            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting users with search: {Search}", search);
            throw;
        }
    }

    public async Task<User?> GetUserAsync(string userId)
    {
        try
        {
            var user = await _graphClient.Users[userId].GetAsync(requestConfiguration =>
            {
                requestConfiguration.QueryParameters.Select = new[] { "id", "userPrincipalName", "displayName", "mail", "accountEnabled", "assignedLicenses" };
            });

            return user;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting user: {UserId}", userId);
            throw;
        }
    }

    public async Task<GroupCollectionResponse?> GetGroupsAsync(int top = 50, string? search = null)
    {
        try
        {
            var response = await _graphClient.Groups.GetAsync(requestConfiguration =>
            {
                requestConfiguration.QueryParameters.Top = top;
                requestConfiguration.QueryParameters.Select = new[] { "id", "displayName", "description", "groupTypes", "mailEnabled", "securityEnabled" };

                if (!string.IsNullOrEmpty(search))
                {
                    requestConfiguration.QueryParameters.Search = $"\"displayName:{search}\"";
                    requestConfiguration.Headers.Add("ConsistencyLevel", "eventual");
                }
            });

            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting groups with search: {Search}", search);
            throw;
        }
    }

    public async Task<Group?> GetGroupAsync(string groupId)
    {
        try
        {
            var group = await _graphClient.Groups[groupId].GetAsync(requestConfiguration =>
            {
                requestConfiguration.QueryParameters.Select = new[] { "id", "displayName", "description", "groupTypes", "mailEnabled", "securityEnabled" };
            });

            return group;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting group: {GroupId}", groupId);
            throw;
        }
    }

    public async Task<Dictionary<string, string>> GetGroupNamesAsync(IEnumerable<string> groupIds)
    {
        var groupNames = new Dictionary<string, string>();

        try
        {
            var tasks = groupIds.Select(async groupId =>
            {
                try
                {
                    var group = await GetGroupAsync(groupId);
                    return new { GroupId = groupId, GroupName = group?.DisplayName ?? $"Unknown Group ({groupId})" };
                }
                catch
                {
                    return new { GroupId = groupId, GroupName = $"Unknown Group ({groupId})" };
                }
            });

            var results = await Task.WhenAll(tasks);

            foreach (var result in results)
            {
                groupNames[result.GroupId] = result.GroupName;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting group names for IDs: {GroupIds}", string.Join(", ", groupIds));
        }

        return groupNames;
    }
}