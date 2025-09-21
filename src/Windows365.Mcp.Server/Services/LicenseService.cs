using Microsoft.Graph;
using Microsoft.Graph.Models;
using Microsoft.Graph.Users.Item.AssignLicense;
using Windows365.Mcp.Server.Models;

namespace Windows365.Mcp.Server.Services;

public class LicenseService
{
    private readonly GraphServiceClient _graphClient;
    private readonly ILogger<LicenseService> _logger;

    // Windows 365 SKU patterns for flexible matching
    private readonly string[] _windows365SkuPatterns = new[]
    {
        "CPC_E_",              // Windows 365 Enterprise editions (standard)
        "CPC_B_",              // Windows 365 Business editions (standard)
        "CPC_S_",              // Windows 365 Shared Use editions
        "CPC_SS_",             // Windows 365 Business (alternative format)
        "CPC_LVL_",            // Windows 365 Enterprise (preview/level format)
        "CPC_1",               // Windows 365 Enterprise numbered format
        "CPC_2",               // Windows 365 Enterprise numbered format
        "WHB",                 // Windows Hybrid Benefit suffix
        "Windows_365_Business", // Descriptive Business format
        "Windows_365_S_",      // Descriptive Shared Use format
        "CPC_ENTERPRISE",      // Alternative Enterprise naming
        "CPC_BUSINESS"         // Alternative Business naming
    };

    public LicenseService(GraphServiceClient graphClient, ILogger<LicenseService> logger)
    {
        _graphClient = graphClient;
        _logger = logger;
    }

    public async Task<IEnumerable<LicenseInfo>> CheckUserLicensesAsync(string userId)
    {
        try
        {
            var user = await _graphClient.Users[userId].GetAsync(requestConfiguration =>
            {
                requestConfiguration.QueryParameters.Select = new[] { "assignedLicenses" };
            });

            if (user?.AssignedLicenses == null)
                return Enumerable.Empty<LicenseInfo>();

            // Get all subscribed SKUs once
            var subscribedSkusResponse = await _graphClient.SubscribedSkus.GetAsync();
            var allSubscribedSkus = subscribedSkusResponse?.Value ?? new List<Microsoft.Graph.Models.SubscribedSku>();

            var licenses = new List<LicenseInfo>();
            foreach (var license in user.AssignedLicenses)
            {
                if (license.SkuId.HasValue)
                {
                    // Find matching subscribed SKU
                    var subscribedSku = allSubscribedSkus.FirstOrDefault(sku => sku.SkuId == license.SkuId.Value);
                    if (subscribedSku != null)
                    {
                        licenses.Add(MapToLicenseInfo(subscribedSku));
                    }
                }
            }

            return licenses;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking licenses for user: {UserId}", userId);
            throw;
        }
    }

    public async Task<bool> HasWindows365LicenseAsync(string userId)
    {
        try
        {
            var licenses = await CheckUserLicensesAsync(userId);
            return licenses.Any(l => IsWindows365Sku(l.SkuPartNumber));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking Windows 365 license for user: {UserId}", userId);
            throw;
        }
    }

    public async Task<ToolResponse<object>> AssignWindows365LicenseAsync(string userId, string? licenseSkuId = null)
    {
        try
        {
            // Get available Windows 365 licenses if no specific SKU provided
            if (string.IsNullOrEmpty(licenseSkuId))
            {
                var availableLicenses = await GetAvailableWindows365LicensesAsync();
                var availableLicense = availableLicenses.FirstOrDefault();
                
                if (availableLicense == null)
                {
                    return new ToolResponse<object>
                    {
                        Success = false,
                        Error = "No available Windows 365 licenses found"
                    };
                }
                
                licenseSkuId = availableLicense.SkuId;
            }

            // Assign the license
            var requestBody = new AssignLicensePostRequestBody
            {
                AddLicenses = new List<AssignedLicense>
                {
                    new AssignedLicense
                    {
                        SkuId = Guid.Parse(licenseSkuId)
                    }
                },
                RemoveLicenses = new List<Guid?>()
            };

            await _graphClient.Users[userId].AssignLicense.PostAsync(requestBody);

            return new ToolResponse<object>
            {
                Success = true,
                Message = $"Windows 365 license assigned to user {userId}",
                Data = new { UserId = userId, LicenseSkuId = licenseSkuId }
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error assigning Windows 365 license to user: {UserId}", userId);
            return new ToolResponse<object>
            {
                Success = false,
                Error = $"Error assigning license: {ex.Message}"
            };
        }
    }

    public async Task<IEnumerable<LicenseInfo>> GetAvailableWindows365LicensesAsync()
    {
        try
        {
            var response = await _graphClient.SubscribedSkus.GetAsync();

            return response?.Value?
                .Where(sku => IsWindows365Sku(sku.SkuPartNumber))
                .Where(sku => (sku.PrepaidUnits?.Enabled ?? 0) > (sku.ConsumedUnits ?? 0))
                .Select(MapToLicenseInfo) ?? Enumerable.Empty<LicenseInfo>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting available Windows 365 licenses");
            throw;
        }
    }

    public async Task<IEnumerable<LicenseInfo>> GetAllWindows365LicensesAsync()
    {
        try
        {
            var response = await _graphClient.SubscribedSkus.GetAsync();

            return response?.Value?
                .Where(sku => IsWindows365Sku(sku.SkuPartNumber))
                .Select(MapToLicenseInfo) ?? Enumerable.Empty<LicenseInfo>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting all Windows 365 licenses");
            throw;
        }
    }

    public async Task<ToolResponse<object>> UnassignWindows365LicenseAsync(string userId, string? licenseSkuId = null)
    {
        try
        {
            // Get user's current Windows 365 licenses if no specific SKU provided
            if (string.IsNullOrEmpty(licenseSkuId))
            {
                // Get user's assigned licenses directly
                var user = await _graphClient.Users[userId].GetAsync(requestConfiguration =>
                {
                    requestConfiguration.QueryParameters.Select = new[] { "assignedLicenses" };
                });

                if (user?.AssignedLicenses == null || !user.AssignedLicenses.Any())
                {
                    return new ToolResponse<object>
                    {
                        Success = false,
                        Error = "User does not have any licenses assigned"
                    };
                }

                // Get all subscribed SKUs once (same pattern as CheckUserLicensesAsync)
                var subscribedSkusResponse = await _graphClient.SubscribedSkus.GetAsync();
                var allSubscribedSkus = subscribedSkusResponse?.Value ?? new List<Microsoft.Graph.Models.SubscribedSku>();

                // Find Windows 365 licenses from user's assigned licenses
                Guid? windows365SkuId = null;

                foreach (var assignedLicense in user.AssignedLicenses)
                {
                    if (assignedLicense.SkuId.HasValue)
                    {
                        // Find matching subscribed SKU from the cached list
                        var subscribedSku = allSubscribedSkus.FirstOrDefault(sku => sku.SkuId == assignedLicense.SkuId.Value);
                        if (subscribedSku != null && IsWindows365Sku(subscribedSku.SkuPartNumber))
                        {
                            _logger.LogInformation($"Found Windows 365 license for unassignment: {subscribedSku.SkuPartNumber}");
                            windows365SkuId = assignedLicense.SkuId.Value;
                            break;
                        }
                    }
                }

                if (!windows365SkuId.HasValue)
                {
                    return new ToolResponse<object>
                    {
                        Success = false,
                        Error = $"User does not have any Windows 365 licenses assigned. Found {user.AssignedLicenses.Count} total licenses."
                    };
                }

                licenseSkuId = windows365SkuId.Value.ToString();
            }

            // Remove the license
            var requestBody = new AssignLicensePostRequestBody
            {
                AddLicenses = new List<AssignedLicense>(),
                RemoveLicenses = new List<Guid?> { Guid.Parse(licenseSkuId) }
            };

            await _graphClient.Users[userId].AssignLicense.PostAsync(requestBody);

            return new ToolResponse<object>
            {
                Success = true,
                Message = $"Windows 365 license unassigned from user {userId}",
                Data = new { UserId = userId, LicenseSkuId = licenseSkuId }
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error unassigning Windows 365 license from user: {UserId}", userId);
            return new ToolResponse<object>
            {
                Success = false,
                Error = $"Error unassigning license: {ex.Message}"
            };
        }
    }

    private static LicenseInfo MapToLicenseInfo(SubscribedSku subscribedSku)
    {
        return new LicenseInfo
        {
            SkuId = subscribedSku.SkuId?.ToString() ?? string.Empty,
            SkuPartNumber = subscribedSku.SkuPartNumber ?? string.Empty,
            CapabilityStatus = subscribedSku.CapabilityStatus ?? string.Empty,
            ConsumedUnits = subscribedSku.ConsumedUnits ?? 0,
            PrepaidUnits = new PrepaidUnits
            {
                Enabled = subscribedSku.PrepaidUnits?.Enabled ?? 0,
                Suspended = subscribedSku.PrepaidUnits?.Suspended ?? 0,
                Warning = subscribedSku.PrepaidUnits?.Warning ?? 0
            },
            ServicePlans = subscribedSku.ServicePlans?.Select(sp => new ServicePlan
            {
                ServicePlanId = sp.ServicePlanId?.ToString() ?? string.Empty,
                ServicePlanName = sp.ServicePlanName ?? string.Empty,
                ProvisioningStatus = sp.ProvisioningStatus ?? string.Empty,
                AppliesTo = sp.AppliesTo ?? string.Empty
            }).ToList() ?? new List<ServicePlan>()
        };
    }

    private bool IsWindows365Sku(string? skuPartNumber)
    {
        if (string.IsNullOrEmpty(skuPartNumber))
            return false;

        // Check if it contains any of our Windows 365 patterns using Contains (handles Unicode properly)
        var isWindows365 = _windows365SkuPatterns.Any(pattern =>
            skuPartNumber.Contains(pattern, StringComparison.OrdinalIgnoreCase));

        if (isWindows365)
            _logger.LogDebug($"Identified Windows 365 SKU: '{skuPartNumber}'");

        return isWindows365;
    }
}