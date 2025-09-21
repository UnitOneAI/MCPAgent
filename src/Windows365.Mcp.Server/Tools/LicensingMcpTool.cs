using Windows365.Mcp.Server.Services;

namespace Windows365.Mcp.Server.Tools;

[McpServerToolType]
public static class LicensingMcpTool
{

    [McpServerTool, Description("Check Windows 365 license assignments for a specific user")]
    public static async Task<string> CheckUserLicenses(
        LicenseService licenseService,
        [Description("The user principal name or user ID to check licenses for")] string userId)
    {
        try
        {
            var userLicenses = await licenseService.CheckUserLicensesAsync(userId);
            
            var result = new
            {
                success = true,
                userId = userId,
                count = userLicenses.Count(),
                licenses = userLicenses.Select(license => new
                {
                    skuId = license.SkuId,
                    skuPartNumber = license.SkuPartNumber,
                    capabilityStatus = license.CapabilityStatus,
                    servicePlans = license.ServicePlans
                })
            };

            return JsonSerializer.Serialize(result, new JsonSerializerOptions { WriteIndented = true });
        }
        catch (Exception ex)
        {
            var errorResult = new
            {
                success = false,
                error = ex.Message,
                type = "User License Check Error",
                userId = userId
            };
            
            return JsonSerializer.Serialize(errorResult, new JsonSerializerOptions { WriteIndented = true });
        }
    }

    [McpServerTool, Description("Get Windows 365 licenses in the tenant including availability information for consumed and available licenses")]
    public static async Task<string> GetWindows365Licenses(LicenseService licenseService)
    {
        try
        {
            var licenses = await licenseService.GetAllWindows365LicensesAsync();

            var result = new
            {
                success = true,
                count = licenses.Count(),
                licenses = licenses.Select(license => new
                {
                    skuId = license.SkuId,
                    skuPartNumber = license.SkuPartNumber,
                    capabilityStatus = license.CapabilityStatus,
                    consumedUnits = license.ConsumedUnits,
                    prepaidUnits = license.PrepaidUnits,
                    available = (license.PrepaidUnits?.Enabled ?? 0) - license.ConsumedUnits,
                    servicePlans = license.ServicePlans
                })
            };

            return JsonSerializer.Serialize(result, new JsonSerializerOptions { WriteIndented = true });
        }
        catch (Exception ex)
        {
            var errorResult = new
            {
                success = false,
                error = ex.Message,
                type = "License Retrieval Error"
            };

            return JsonSerializer.Serialize(errorResult, new JsonSerializerOptions { WriteIndented = true });
        }
    }

    [McpServerTool(Destructive = true), Description("Assign a Windows 365 license to a user")]
    public static async Task<string> AssignLicense(
        LicenseService licenseService,
        [Description("The user principal name or user ID to assign the license to")] string userId,
        [Description("The SKU ID of the license to assign (optional - will auto-select if not provided)")] string? licenseSkuId = null)
    {
        try
        {
            var response = await licenseService.AssignWindows365LicenseAsync(userId, licenseSkuId);

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

    [McpServerTool(Destructive = true), Description("Unassign a Windows 365 license from a user")]
    public static async Task<string> UnassignLicense(
        LicenseService licenseService,
        [Description("The user principal name or user ID to unassign the license from")] string userId,
        [Description("The SKU ID of the license to unassign (optional - will auto-select if not provided)")] string? licenseSkuId = null)
    {
        try
        {
            var response = await licenseService.UnassignWindows365LicenseAsync(userId, licenseSkuId);

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

}