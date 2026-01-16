using Azure.Storage.Files.DataLake.Models;

namespace ClarityDQ.OneLake;

public class OneLakeConfig
{
    public string ConnectionString { get; set; } = string.Empty;
    public string FileSystemName { get; set; } = "claritydq-results";
    public string WorkspaceId { get; set; } = string.Empty;
    public bool Enabled { get; set; } = false;
}

public static class OneLakeExtensions
{
    public static string GetOneLakePath(this string workspaceId, string category, params string[] segments)
    {
        var path = $"{category}/{workspaceId}";
        if (segments.Length > 0)
        {
            path += "/" + string.Join("/", segments);
        }
        return path;
    }
}
