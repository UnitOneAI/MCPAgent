using Windows365.Mcp.Server.Services;

namespace Windows365.Mcp.Server.Tools;

[McpServerToolType]
public static class CloudPCManagementMcpTool
{

    [McpServerTool(Destructive = true), Description("End the grace period for a specific Cloud PC immediately")]
    public static async Task<string> EndGracePeriod(
        CloudPCService cloudPCService,
        [Description("The ID of the Cloud PC to end grace period for")] string cloudPcId)
    {
        try
        {
            var response = await cloudPCService.EndGracePeriodAsync(cloudPcId);

            var result = new
            {
                success = response.Success,
                message = response.Message ?? response.Error
            };

            return JsonSerializer.Serialize(result, new JsonSerializerOptions { WriteIndented = true });
        }
        catch (Exception ex)
        {
            var errorResult = new
            {
                success = false,
                error = ex.Message
            };

            return JsonSerializer.Serialize(errorResult, new JsonSerializerOptions { WriteIndented = true });
        }
    }

    [McpServerTool(Destructive = true), Description("Restart a specific Cloud PC")]
    public static async Task<string> RebootCloudPC(
        CloudPCService cloudPCService,
        [Description("The ID of the Cloud PC to reboot")] string cloudPcId)
    {
        try
        {
            await cloudPCService.RebootCloudPCAsync(cloudPcId);

            var result = new
            {
                success = true,
                message = "Cloud PC reboot initiated successfully"
            };

            return JsonSerializer.Serialize(result, new JsonSerializerOptions { WriteIndented = true });
        }
        catch (Exception ex)
        {
            var errorResult = new
            {
                success = false,
                error = ex.Message
            };

            return JsonSerializer.Serialize(errorResult, new JsonSerializerOptions { WriteIndented = true });
        }
    }
}