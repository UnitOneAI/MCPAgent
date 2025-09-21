using System.Text.Json;
using System.Text;

namespace Windows365.Mcp.Server.Utilities;

/// <summary>
/// Helper class for implementing MCP-compliant cursor-based pagination
/// </summary>
public static class PaginationHelper
{
    /// <summary>
    /// Create an opaque cursor from pagination state
    /// </summary>
    /// <param name="skip">Number of items to skip</param>
    /// <param name="pageSize">Size of the page</param>
    /// <param name="totalCount">Total count of items</param>
    /// <returns>Base64-encoded opaque cursor</returns>
    public static string CreateCursor(int skip, int pageSize, int totalCount)
    {
        var cursorData = new
        {
            s = skip,  // skip
            p = pageSize,  // page size
            t = totalCount,  // total count
            v = 1  // version for future compatibility
        };
        
        var json = JsonSerializer.Serialize(cursorData);
        var bytes = Encoding.UTF8.GetBytes(json);
        return Convert.ToBase64String(bytes);
    }
    
    /// <summary>
    /// Parse an opaque cursor to extract pagination state
    /// </summary>
    /// <param name="cursor">Base64-encoded cursor</param>
    /// <returns>Pagination state or null if invalid</returns>
    public static (int skip, int pageSize, int totalCount)? ParseCursor(string cursor)
    {
        try
        {
            var bytes = Convert.FromBase64String(cursor);
            var json = Encoding.UTF8.GetString(bytes);
            var cursorData = JsonSerializer.Deserialize<CursorData>(json);
            
            if (cursorData?.v == 1) // Version check
            {
                return (cursorData.s, cursorData.p, cursorData.t);
            }
        }
        catch
        {
            // Invalid cursor format
        }
        
        return null;
    }
    
    /// <summary>
    /// Create pagination result with cursor support
    /// </summary>
    /// <typeparam name="T">Type of items being paginated</typeparam>
    /// <param name="allItems">All items to paginate</param>
    /// <param name="cursor">Current cursor (null for first page)</param>
    /// <param name="defaultPageSize">Default page size</param>
    /// <returns>Paginated result with next cursor if applicable</returns>
    public static PaginatedResult<T> Paginate<T>(
        IEnumerable<T> allItems, 
        string? cursor = null, 
        int defaultPageSize = 50)
    {
        var itemsList = allItems.ToList();
        var totalCount = itemsList.Count;
        
        int skip = 0;
        int pageSize = defaultPageSize;
        
        // Parse cursor if provided
        if (!string.IsNullOrEmpty(cursor))
        {
            var parsed = ParseCursor(cursor);
            if (parsed.HasValue)
            {
                skip = parsed.Value.skip;
                pageSize = parsed.Value.pageSize;
            }
            else
            {
                throw new ArgumentException("Invalid cursor format", nameof(cursor));
            }
        }
        
        // Get current page
        var currentPage = itemsList.Skip(skip).Take(pageSize).ToList();
        
        // Calculate next cursor
        string? nextCursor = null;
        var nextSkip = skip + pageSize;
        if (nextSkip < totalCount)
        {
            nextCursor = CreateCursor(nextSkip, pageSize, totalCount);
        }
        
        return new PaginatedResult<T>
        {
            Items = currentPage,
            NextCursor = nextCursor,
            TotalCount = totalCount,
            CurrentPage = skip / pageSize + 1,
            PageSize = pageSize,
            HasMore = nextCursor != null
        };
    }
    
    private class CursorData
    {
        public int s { get; set; } // skip
        public int p { get; set; } // page size  
        public int t { get; set; } // total count
        public int v { get; set; } // version
    }
}

/// <summary>
/// Result of a paginated operation
/// </summary>
/// <typeparam name="T">Type of items</typeparam>
public class PaginatedResult<T>
{
    public IList<T> Items { get; set; } = new List<T>();
    public string? NextCursor { get; set; }
    public int TotalCount { get; set; }
    public int CurrentPage { get; set; }
    public int PageSize { get; set; }
    public bool HasMore { get; set; }
}