using Azure.Identity;
using Microsoft.Graph;

namespace Windows365.Mcp.Server.Authentication;

public class GraphAuthenticationProvider
{
    private readonly DefaultAzureCredential _credential;
    private readonly string[] _scopes;

    public GraphAuthenticationProvider(string clientId, string tenantId, string clientSecret, string[] scopes)
    {
        var options = new DefaultAzureCredentialOptions
        {
            TenantId = tenantId,
            ExcludeEnvironmentCredential = false,
            ExcludeManagedIdentityCredential = false,
            ExcludeAzureCliCredential = false,
            ExcludeVisualStudioCredential = false,
            ExcludeInteractiveBrowserCredential = false,
            InteractiveBrowserCredentialClientId = clientId,
            InteractiveBrowserTenantId = tenantId
        };

        _credential = new DefaultAzureCredential(options);
        _scopes = scopes;
    }

    public GraphServiceClient GetGraphServiceClient()
    {
        return new GraphServiceClient(_credential, _scopes);
    }
}