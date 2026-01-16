using Azure.Core;
using Azure.Identity;
using System.Net.Http.Headers;
using System.Text.Json;

namespace ClarityDQ.FabricClient;

public interface IFabricClient
{
    Task<FabricWorkspace[]> GetWorkspacesAsync(CancellationToken cancellationToken = default);
    Task<FabricItem[]> GetWorkspaceItemsAsync(string workspaceId, CancellationToken cancellationToken = default);
    Task<FabricTableSchema> GetTableSchemaAsync(string workspaceId, string lakehouseId, string tableName, CancellationToken cancellationToken = default);
}

public class FabricClient : IFabricClient
{
    private readonly HttpClient _httpClient;
    private readonly TokenCredential _credential;
    private readonly FabricClientOptions _options;

    public FabricClient(HttpClient httpClient, FabricClientOptions options)
    {
        _httpClient = httpClient;
        _options = options;
        _credential = new ClientSecretCredential(
            options.TenantId,
            options.ClientId,
            options.ClientSecret);
        
        _httpClient.BaseAddress = new Uri(options.FabricApiBaseUrl);
    }

    public async Task<FabricWorkspace[]> GetWorkspacesAsync(CancellationToken cancellationToken = default)
    {
        await EnsureAuthenticatedAsync(cancellationToken);
        
        var response = await _httpClient.GetAsync("workspaces", cancellationToken);
        response.EnsureSuccessStatusCode();
        
        var content = await response.Content.ReadAsStringAsync(cancellationToken);
        var result = JsonSerializer.Deserialize<FabricWorkspacesResponse>(content);
        
        return result?.Value ?? Array.Empty<FabricWorkspace>();
    }

    public async Task<FabricItem[]> GetWorkspaceItemsAsync(string workspaceId, CancellationToken cancellationToken = default)
    {
        await EnsureAuthenticatedAsync(cancellationToken);
        
        var response = await _httpClient.GetAsync($"workspaces/{workspaceId}/items", cancellationToken);
        response.EnsureSuccessStatusCode();
        
        var content = await response.Content.ReadAsStringAsync(cancellationToken);
        var result = JsonSerializer.Deserialize<FabricItemsResponse>(content);
        
        return result?.Value ?? Array.Empty<FabricItem>();
    }

    public async Task<FabricTableSchema> GetTableSchemaAsync(string workspaceId, string lakehouseId, string tableName, CancellationToken cancellationToken = default)
    {
        await EnsureAuthenticatedAsync(cancellationToken);
        
        var response = await _httpClient.GetAsync(
            $"workspaces/{workspaceId}/lakehouses/{lakehouseId}/tables/{tableName}",
            cancellationToken);
        
        response.EnsureSuccessStatusCode();
        
        var content = await response.Content.ReadAsStringAsync(cancellationToken);
        var result = JsonSerializer.Deserialize<FabricTableSchema>(content);
        
        return result ?? new FabricTableSchema();
    }

    private async Task EnsureAuthenticatedAsync(CancellationToken cancellationToken)
    {
        var token = await _credential.GetTokenAsync(
            new TokenRequestContext(new[] { "https://api.fabric.microsoft.com/.default" }),
            cancellationToken);
        
        _httpClient.DefaultRequestHeaders.Authorization = 
            new AuthenticationHeaderValue("Bearer", token.Token);
    }
}

public class FabricWorkspacesResponse
{
    public FabricWorkspace[] Value { get; set; } = Array.Empty<FabricWorkspace>();
}

public class FabricWorkspace
{
    public string Id { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
}

public class FabricItemsResponse
{
    public FabricItem[] Value { get; set; } = Array.Empty<FabricItem>();
}

public class FabricItem
{
    public string Id { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
}

public class FabricTableSchema
{
    public string Name { get; set; } = string.Empty;
    public FabricColumn[] Columns { get; set; } = Array.Empty<FabricColumn>();
}

public class FabricColumn
{
    public string Name { get; set; } = string.Empty;
    public string DataType { get; set; } = string.Empty;
    public bool IsNullable { get; set; }
}
