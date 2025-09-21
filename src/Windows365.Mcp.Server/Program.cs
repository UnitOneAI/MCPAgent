using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using ModelContextProtocol.Server;
using Windows365.Mcp.Server.Authentication;
using Windows365.Mcp.Server.Services;
using DotNetEnv;

namespace Windows365.Mcp.Server;

class Program
{
    static async Task Main(string[] args)
    {
        var envPaths = new[] {
            Environment.GetEnvironmentVariable("WINDOWS365_ENV_PATH"),
            ".env",
            "../../.env"
        }.Where(path => !string.IsNullOrEmpty(path));

        foreach (var envPath in envPaths)
        {
            if (File.Exists(envPath))
            {
                Env.Load(envPath);
                break;
            }
        }

        var builder = Host.CreateApplicationBuilder(args);
        
        // Configure logging to use stderr (MCP requirement)
        builder.Logging.ClearProviders();
        builder.Logging.AddConsole(options =>
        {
            options.LogToStandardErrorThreshold = LogLevel.Information; // Information and above to stderr
        });
        builder.Logging.SetMinimumLevel(LogLevel.Information);
        
        // Configure Azure authentication
        var clientId = Environment.GetEnvironmentVariable("AZURE_CLIENT_ID") ??
                      throw new InvalidOperationException("Azure Client ID not configured");

        var tenantId = Environment.GetEnvironmentVariable("AZURE_TENANT_ID") ??
                      throw new InvalidOperationException("Azure Tenant ID not configured");

        var clientSecret = Environment.GetEnvironmentVariable("AZURE_CLIENT_SECRET") ??
                          throw new InvalidOperationException("Azure Client Secret not configured");

        var scopes = new[] { "https://graph.microsoft.com/.default" };

        // Register Azure/Graph services
        builder.Services.AddSingleton(provider => new GraphAuthenticationProvider(clientId, tenantId, clientSecret, scopes));
        builder.Services.AddSingleton(provider => 
        {
            var authProvider = provider.GetRequiredService<GraphAuthenticationProvider>();
            return authProvider.GetGraphServiceClient();
        });
        builder.Services.AddSingleton<CloudPCService>();
        builder.Services.AddSingleton<LicenseService>();
        builder.Services.AddSingleton<ProvisioningService>();
        builder.Services.AddSingleton<ProvisioningPolicyService>();
        builder.Services.AddSingleton<UserService>();

        // Configure MCP Server with official SDK
        builder.Services
            .AddMcpServer()
            .WithStdioServerTransport() // Use STDIO transport as required by MCP spec
            .WithToolsFromAssembly()   // Auto-discover tools with [McpServerTool] attributes
            .WithPromptsFromAssembly() // Auto-discover prompts with [McpServerPrompt] attributes
            .WithResourcesFromAssembly() // Auto-discover resources with [McpServerResource] attributes
            .WithCompleteHandler(async (ctx, ct) =>
            {
                // Windows 365 specific completions
                var windows365Completions = new Dictionary<string, IEnumerable<string>>
                {
                    // Cloud PC troubleshooting completions
                    { "issue", new[] { 
                        "slow performance", "connection failed", "black screen", "audio issues", 
                        "printing problems", "app crashes", "login failure", "network connectivity",
                        "file sync issues", "display resolution", "keyboard not working", "mouse lag"
                    }},
                    
                    // Provisioning policy optimization completions
                    { "focusArea", new[] { "performance", "cost", "security", "compliance", "user experience" }},
                    
                    // Deployment planning completions
                    { "industry", new[] { 
                        "healthcare", "finance", "education", "manufacturing", "retail", 
                        "government", "technology", "legal", "consulting", "general"
                    }},
                    
                    // Common user counts for deployment planning
                    { "userCount", new[] { "10", "25", "50", "100", "250", "500", "1000", "2500", "5000" }},
                    
                    // Timeline options in weeks
                    { "timelineWeeks", new[] { "1", "2", "4", "6", "8", "12", "16", "20", "24" }},
                    
                    // Organization sizes for licensing
                    { "organizationSize", new[] { "10", "25", "50", "100", "250", "500", "1000", "2500", "5000", "10000" }},
                    
                    // Cloud PC IDs (dynamic - would normally be fetched from service)
                    { "cloudPcId", new[] { "example-cloudpc-1", "example-cloudpc-2", "example-cloudpc-3" }},
                    
                    // User principal names (examples)
                    { "userPrincipalName", new[] { 
                        "user@contoso.com", "admin@contoso.com", "manager@contoso.com",
                        "developer@contoso.com", "analyst@contoso.com"
                    }},
                    
                    // Resource URI completions for resource references
                    { "id", new[] { "1", "2", "3", "4", "5" }},
                    { "userId", new[] { "user1@contoso.com", "user2@contoso.com", "admin@contoso.com" }}
                };

                var argument = ctx.Params?.Argument?.Name;
                var value = ctx.Params?.Argument?.Value ?? "";

                if (argument == null || !windows365Completions.ContainsKey(argument))
                {
                    return new ModelContextProtocol.Protocol.CompleteResult
                    {
                        Completion = new ModelContextProtocol.Protocol.Completion
                        {
                            Values = new List<string>(),
                            Total = 0,
                            HasMore = false
                        }
                    };
                }

                // Filter completions based on partial input (case-insensitive prefix match)
                var matchingCompletions = windows365Completions[argument]
                    .Where(completion => completion.StartsWith(value, StringComparison.OrdinalIgnoreCase))
                    .Take(100) // Limit to 100 as per MCP spec
                    .ToList();

                return new ModelContextProtocol.Protocol.CompleteResult
                {
                    Completion = new ModelContextProtocol.Protocol.Completion
                    {
                        Values = matchingCompletions,
                        Total = matchingCompletions.Count,
                        HasMore = matchingCompletions.Count >= 100
                    }
                };
            });

        var host = builder.Build();

        var logger = host.Services.GetRequiredService<ILogger<Program>>();
        logger.LogInformation("Windows 365 MCP Server starting...");

        try
        {
            logger.LogInformation("Starting MCP Server with STDIO transport...");
            await host.RunAsync();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error starting Windows 365 MCP Server");
            throw;
        }
    }
}